using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleServiceHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VehicleServiceHistoryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/VehicleServiceHistory/Vehicle/{vehicleId}
        [HttpGet("Vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<ServiceHistoryDTO>>> GetVehicleServiceHistory(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.ServiceHistory)
                    .ThenInclude(sh => sh.ServiceCenter)
                .Include(v => v.ServiceHistory)
                    .ThenInclude(sh => sh.ServicedByUser)
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            var serviceHistoryDTOs = vehicle.ServiceHistory.Select(sh => new ServiceHistoryDTO
            {
                ServiceHistoryId = sh.ServiceHistoryId,
                VehicleId = sh.VehicleId,
                ServiceType = sh.ServiceType,
                Description = sh.Description,
                ServiceCenterId = sh.ServiceCenterId,
                ServicedByUserId = sh.ServicedByUserId,
                ServiceCenterName = sh.ServiceCenter?.Station_name,
                ServicedByUserName = sh.ServicedByUser != null ? $"{sh.ServicedByUser.FirstName} {sh.ServicedByUser.LastName}" : null,
                ServiceDate = sh.ServiceDate,
                Cost = sh.Cost,
                Mileage = sh.Mileage,
                IsVerified = sh.IsVerified,
                ExternalServiceCenterName = sh.ExternalServiceCenterName,
                ReceiptDocumentPath = sh.ReceiptDocumentPath
            });

            return Ok(serviceHistoryDTOs);
        }

        // GET: api/VehicleServiceHistory/{vehicleId}/{serviceHistoryId}
        [HttpGet("{vehicleId}/{serviceHistoryId}")]
        public async Task<ActionResult<ServiceHistoryDTO>> GetServiceHistory(int vehicleId, int serviceHistoryId)
        {
            var serviceHistory = await _context.VehicleServiceHistories
                .Include(sh => sh.ServiceCenter)
                .Include(sh => sh.ServicedByUser)
                .Include(sh => sh.Vehicle)
                .FirstOrDefaultAsync(sh => sh.ServiceHistoryId == serviceHistoryId && sh.VehicleId == vehicleId);

            if (serviceHistory == null)
            {
                return NotFound("Service history record not found");
            }

            var serviceHistoryDTO = new ServiceHistoryDTO
            {
                ServiceHistoryId = serviceHistory.ServiceHistoryId,
                VehicleId = serviceHistory.VehicleId,
                ServiceType = serviceHistory.ServiceType,
                Description = serviceHistory.Description,
                ServiceCenterId = serviceHistory.ServiceCenterId,
                ServicedByUserId = serviceHistory.ServicedByUserId,
                ServiceCenterName = serviceHistory.ServiceCenter?.Station_name,
                ServicedByUserName = serviceHistory.ServicedByUser != null ? $"{serviceHistory.ServicedByUser.FirstName} {serviceHistory.ServicedByUser.LastName}" : null,
                ServiceDate = serviceHistory.ServiceDate,
                Cost = serviceHistory.Cost,
                Mileage = serviceHistory.Mileage,
                IsVerified = serviceHistory.IsVerified,
                ExternalServiceCenterName = serviceHistory.ExternalServiceCenterName,
                ReceiptDocumentPath = serviceHistory.ReceiptDocumentPath
            };

            return Ok(serviceHistoryDTO);
        }

        // POST: api/VehicleServiceHistory/{vehicleId}
        [HttpPost("{vehicleId}")]
        public async Task<ActionResult<ServiceHistoryDTO>> AddServiceHistory(int vehicleId, [FromBody] AddServiceHistoryDTO dto)
        {
            // Validate vehicle exists
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            // Ensure the DTO uses the path parameter
            dto.VehicleId = vehicleId;

            // Determine if this is a verified service
            bool isVerified = false;

            // If a service center ID is provided, validate it exists and mark as verified
            if (dto.ServiceCenterId.HasValue)
            {
                var serviceCenter = await _context.ServiceCenters.FindAsync(dto.ServiceCenterId.Value);
                if (serviceCenter == null)
                {
                    return NotFound("Service center not found");
                }

                // For a registered service center, we mark as verified
                isVerified = true;

                // Enforce ServiceType to match the correct ServiceName from the Service table
                // Try to find the service by name (case-insensitive)
                var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceName.ToLower() == dto.ServiceType.ToLower());
                if (service != null)
                {
                    dto.ServiceType = service.ServiceName; // Set to canonical name
                }
                // else: fallback to what was provided (for legacy/external/unmatched cases)
            }

            string? receiptPath = null;

            // Handle receipt document upload
            if (!string.IsNullOrEmpty(dto.ReceiptDocument))
            {
                try
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Path.Combine(_environment.ContentRootPath, "Uploads", "ServiceReceipts");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Get file data from base64 string
                    var fileData = Convert.FromBase64String(dto.ReceiptDocument);

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}.pdf";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    // Save the file
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);

                    // Store relative path in database
                    receiptPath = $"Uploads/ServiceReceipts/{fileName}";
                }
                catch
                {
                    // Log error but continue
                    // In production you would want to handle this more gracefully
                }
            }

            // Create new service history record
            var serviceHistory = new VehicleServiceHistory
            {
                VehicleId = dto.VehicleId,
                ServiceType = dto.ServiceType,
                Description = dto.Description,
                ServiceCenterId = dto.ServiceCenterId,
                ServicedByUserId = dto.ServicedByUserId,
                ServiceDate = dto.ServiceDate,
                Cost = dto.Cost,
                Mileage = dto.Mileage,
                IsVerified = isVerified,
                ExternalServiceCenterName = !isVerified ? dto.ExternalServiceCenterName : null,
                ReceiptDocumentPath = receiptPath,
                Vehicle = vehicle
            };

            _context.VehicleServiceHistories.Add(serviceHistory);
            await _context.SaveChangesAsync();

            // Remove matching reminders for this vehicle and service type
            await RemoveMatchingReminders(vehicleId, serviceHistory.ServiceType);

            // Update vehicle mileage if provided and higher than current
            if (dto.Mileage.HasValue && (!vehicle.Mileage.HasValue || dto.Mileage > vehicle.Mileage))
            {
                vehicle.Mileage = dto.Mileage;
                await _context.SaveChangesAsync();
            }

            if (isVerified)
            {
                var customerId = vehicle.CustomerId;
                var notification = new Notification
                {
                    CustomerId = customerId,
                    Title = "Service Record Verified",
                    Message = $"A new verified service record for {serviceHistory.ServiceType} was added.",
                    Type = "service_history_verified",
                    Priority = "Medium", // Ensure this is set
                    PriorityColor = "#3B82F6",
                    VehicleId = serviceHistory.VehicleId,
                    VehicleRegistrationNumber = vehicle.RegistrationNumber,
                    VehicleBrand = vehicle.Brand,
                    VehicleModel = vehicle.Model,
                    ServiceName = serviceHistory.ServiceType,
                    CustomerName = $"{vehicle.Customer?.FirstName} {vehicle.Customer?.LastName}",
                    CreatedAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // Return created service history
            var result = new ServiceHistoryDTO
            {
                ServiceHistoryId = serviceHistory.ServiceHistoryId,
                VehicleId = serviceHistory.VehicleId,
                ServiceType = serviceHistory.ServiceType,
                Description = serviceHistory.Description,
                ServiceCenterId = serviceHistory.ServiceCenterId,
                ServicedByUserId = serviceHistory.ServicedByUserId,
                ServiceCenterName = serviceHistory.ServiceCenter?.Station_name,
                ServiceDate = serviceHistory.ServiceDate,
                Cost = serviceHistory.Cost,
                Mileage = serviceHistory.Mileage,
                IsVerified = serviceHistory.IsVerified,
                ExternalServiceCenterName = serviceHistory.ExternalServiceCenterName,
                ReceiptDocumentPath = serviceHistory.ReceiptDocumentPath
            };

            return CreatedAtAction(nameof(GetServiceHistory),
                new { vehicleId = serviceHistory.VehicleId, serviceHistoryId = serviceHistory.ServiceHistoryId },
                result);
        }

        // PUT: api/VehicleServiceHistory/{vehicleId}/{serviceHistoryId}
        [HttpPut("{vehicleId}/{serviceHistoryId}")]
        public async Task<IActionResult> UpdateServiceHistory(int vehicleId, int serviceHistoryId, [FromBody] UpdateServiceHistoryDTO dto)
        {
            // Ensure path parameters match DTO
            if (serviceHistoryId != dto.ServiceHistoryId)
            {
                return BadRequest("ServiceHistoryId mismatch");
            }

            var serviceHistory = await _context.VehicleServiceHistories
                .Include(sh => sh.Vehicle)
                .Include(sh => sh.ServiceCenter)
                .FirstOrDefaultAsync(sh => sh.ServiceHistoryId == serviceHistoryId && sh.VehicleId == vehicleId);

            if (serviceHistory == null)
            {
                return NotFound("Service history record not found");
            }

            // Determine if this is a verified service
            bool isVerified = false;

            // If a service center ID is provided, validate it exists and mark as verified
            if (dto.ServiceCenterId.HasValue)
            {
                var serviceCenter = await _context.ServiceCenters.FindAsync(dto.ServiceCenterId.Value);
                if (serviceCenter == null)
                {
                    return NotFound("Service center not found");
                }

                // Mark as verified if a registered service center is used
                isVerified = true;
            }

            string? receiptPath = serviceHistory.ReceiptDocumentPath;

            // Handle receipt document upload if changed
            if (!string.IsNullOrEmpty(dto.ReceiptDocument))
            {
                try
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsDir = Path.Combine(_environment.ContentRootPath, "Uploads", "ServiceReceipts");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    // Delete old receipt if exists
                    if (!string.IsNullOrEmpty(serviceHistory.ReceiptDocumentPath))
                    {
                        var oldFilePath = Path.Combine(_environment.ContentRootPath,
                            serviceHistory.ReceiptDocumentPath.Replace('/', Path.DirectorySeparatorChar));

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Get file data from base64 string
                    var fileData = Convert.FromBase64String(dto.ReceiptDocument);

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}.pdf";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    // Save the file
                    await System.IO.File.WriteAllBytesAsync(filePath, fileData);

                    // Store relative path in database
                    receiptPath = $"Uploads/ServiceReceipts/{fileName}";
                }
                catch
                {
                    // Log error but continue
                    // In production you would want to handle this more gracefully
                }
            }

            // Update service history
            serviceHistory.ServiceType = dto.ServiceType;
            serviceHistory.Description = dto.Description;
            serviceHistory.ServiceCenterId = dto.ServiceCenterId;
            serviceHistory.ServicedByUserId = dto.ServicedByUserId;
            serviceHistory.ServiceDate = dto.ServiceDate;
            serviceHistory.Cost = dto.Cost;
            serviceHistory.Mileage = dto.Mileage;
            serviceHistory.IsVerified = isVerified;
            serviceHistory.ExternalServiceCenterName = !isVerified ? dto.ExternalServiceCenterName : null;
            serviceHistory.ReceiptDocumentPath = receiptPath;

            _context.Entry(serviceHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Update vehicle mileage if provided and higher than current
                var vehicle = serviceHistory.Vehicle;
                if (dto.Mileage.HasValue && (!vehicle.Mileage.HasValue || dto.Mileage > vehicle.Mileage))
                {
                    vehicle.Mileage = dto.Mileage;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceHistoryExists(serviceHistoryId, vehicleId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/VehicleServiceHistory/{vehicleId}/{serviceHistoryId}
        [HttpDelete("{vehicleId}/{serviceHistoryId}")]
        public async Task<IActionResult> DeleteServiceHistory(int vehicleId, int serviceHistoryId)
        {
            var serviceHistory = await _context.VehicleServiceHistories
                .FirstOrDefaultAsync(sh => sh.ServiceHistoryId == serviceHistoryId && sh.VehicleId == vehicleId);

            if (serviceHistory == null)
            {
                return NotFound();
            }

            // Delete receipt file if exists
            if (!string.IsNullOrEmpty(serviceHistory.ReceiptDocumentPath))
            {
                try
                {
                    var filePath = Path.Combine(_environment.ContentRootPath,
                        serviceHistory.ReceiptDocumentPath.Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    // Log error but continue with deletion
                }
            }

            _context.VehicleServiceHistories.Remove(serviceHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Removes all service reminders for a vehicle and service type (case-insensitive)
        private async Task RemoveMatchingReminders(int vehicleId, string serviceType)
        {
            var reminders = await _context.ServiceReminders
                .Include(sr => sr.Service)
                .Where(sr => sr.VehicleId == vehicleId && sr.Service.ServiceName.ToLower() == serviceType.ToLower())
                .ToListAsync();

            if (reminders.Any())
            {
                _context.ServiceReminders.RemoveRange(reminders);
                await _context.SaveChangesAsync();
            }
        }

        // Helper method to check if a service history record exists
        private bool ServiceHistoryExists(int serviceHistoryId, int vehicleId)
        {
            return _context.VehicleServiceHistories.Any(e => e.ServiceHistoryId == serviceHistoryId && e.VehicleId == vehicleId);
        }
    }
}
