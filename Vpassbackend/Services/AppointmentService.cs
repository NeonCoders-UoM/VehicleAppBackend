using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly DailyLimitService _dailyLimitService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AppointmentService> _logger;
        
        public AppointmentService(ApplicationDbContext db, DailyLimitService dailyLimitService, INotificationService notificationService, ILogger<AppointmentService> logger)
        {
            _db = db;
            _dailyLimitService = dailyLimitService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<AppointmentSummaryForCustomerDTO> CreateAppointmentAsync(AppointmentCreateDTO dto)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && v.CustomerId == dto.CustomerId);

            if (vehicle == null)
                throw new KeyNotFoundException("Vehicle not found or does not belong to the customer.");

            var availableServices = await _db.ServiceCenterServices
                .Where(scs => scs.Station_id == dto.Station_id && dto.ServiceIds.Contains(scs.ServiceId))
                .Include(scs => scs.Service)
                .ToListAsync();

            if (availableServices.Count != dto.ServiceIds.Count)
                throw new InvalidOperationException("One or more selected services are not available at the selected station.");

            var appointment = new Appointment
            {
                VehicleId = dto.VehicleId,
                Station_id = dto.Station_id,
                CustomerId = dto.CustomerId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Pending",
                AppointmentPrice = availableServices.Sum(s => s.CustomPrice ?? s.Service.BasePrice ?? 0)
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            foreach (var svc in availableServices)
            {
                _db.AppointmentServices.Add(new Models.AppointmentService
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = svc.ServiceId,
                    ServicePrice = svc.CustomPrice ?? svc.Service.BasePrice ?? 0
                });
            }

            await _db.SaveChangesAsync();

            var stationName = await _db.ServiceCenters
                .Where(sc => sc.Station_id == dto.Station_id)
                .Select(sc => sc.Station_name)
                .FirstOrDefaultAsync() ?? "Unknown";

            return new AppointmentSummaryForCustomerDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                StationName = stationName
            };
        }

        public async Task<List<AppointmentSummaryForCustomerDTO>> GetAppointmentsByCustomerVehicleAsync(int customerId, int vehicleId)
        {
            return await _db.Appointments
                .Include(a => a.ServiceCenter)
                .Where(a => a.CustomerId == customerId && a.VehicleId == vehicleId)
                .Select(a => new AppointmentSummaryForCustomerDTO
                {
                    AppointmentId = a.AppointmentId,
                    StationName = a.ServiceCenter.Station_name,
                    AppointmentDate = a.AppointmentDate ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        public async Task<AppointmentDetailForCustomerDTO> GetCustomerAppointmentDetailsAsync(int customerId, int vehicleId, int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(a => a.ServiceCenter)
                .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.CustomerId == customerId &&
                    a.VehicleId == vehicleId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found for the specified customer and vehicle.");

            var serviceIds = appointment.AppointmentServices.Select(s => s.ServiceId).ToList();

            // Get accurate pricing and loyalty points from ServiceCenterService
            var serviceCenterServices = await _db.ServiceCenterServices
                .Where(scs => scs.Station_id == appointment.Station_id && serviceIds.Contains(scs.ServiceId))
                .Include(scs => scs.Service)
                .ToListAsync();

            // Build service list using pricing from service center mapping
            var serviceList = serviceCenterServices.Select(scs => new AppointmentServiceDetailDTO
            {
                ServiceName = scs.Service.ServiceName,
                EstimatedCost = scs.CustomPrice ?? scs.BasePrice ?? scs.Service.BasePrice ?? 0
            }).ToList();

            // Calculate totals
            var totalCost = serviceList.Sum(s => s.EstimatedCost);
            var totalLoyaltyPoints = serviceCenterServices.Sum(scs => scs.LoyaltyPoints ?? 0);

            return new AppointmentDetailForCustomerDTO
            {
                VehicleRegistration = appointment.Vehicle.RegistrationNumber,
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                ServiceCenterId = appointment.ServiceCenter.Station_id,
                ServiceCenterAddress = appointment.ServiceCenter.Address,
                ServiceCenterName = appointment.ServiceCenter.Station_name,
                DistanceInKm = null, // To be implemented
                LoyaltyPoints = totalLoyaltyPoints,
                Services = serviceList,
                TotalCost = totalCost
            };
        }

        public async Task<List<AppointmentSummaryForAdminDTO>> GetAppointmentsForServiceCenterAsync(int stationId)
        {
            return await _db.Appointments
                .Include(a => a.Customer)
                .Where(a => a.Station_id == stationId)
                .Select(a => new AppointmentSummaryForAdminDTO
                {
                    AppointmentId = a.AppointmentId,
                    OwnerName = (a.Customer != null ? a.Customer.FirstName + " " + a.Customer.LastName : "Unknown"),
                    AppointmentDate = a.AppointmentDate ?? DateTime.MinValue
                })
                .ToListAsync();
        }

        // Calculate cost for services at a specific service center without creating appointment
        public async Task<decimal> CalculateServiceCostAsync(int stationId, List<int> serviceIds)
        {
            var availableServices = await _db.ServiceCenterServices
                .Where(scs => scs.Station_id == stationId && serviceIds.Contains(scs.ServiceId))
                .Include(scs => scs.Service)
                .ToListAsync();

            if (availableServices.Count != serviceIds.Count)
                throw new InvalidOperationException("One or more selected services are not available at the selected station.");

            return availableServices.Sum(s => s.CustomPrice ?? s.Service.BasePrice ?? 0);
        }

        // Create appointment only when user confirms (for final booking)
        public async Task<AppointmentSummaryForCustomerDTO> CreateConfirmedAppointmentAsync(AppointmentCreateDTO dto)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == dto.VehicleId && v.CustomerId == dto.CustomerId);

            if (vehicle == null)
                throw new KeyNotFoundException("Vehicle not found or does not belong to the customer.");

            var availableServices = await _db.ServiceCenterServices
                .Where(scs => scs.Station_id == dto.Station_id && dto.ServiceIds.Contains(scs.ServiceId))
                .Include(scs => scs.Service)
                .ToListAsync();

            if (availableServices.Count != dto.ServiceIds.Count)
                throw new InvalidOperationException("One or more selected services are not available at the selected station.");

            // Check if service center has available slots for this date
            var canBook = await _dailyLimitService.CanBookAppointmentAsync(dto.Station_id, dto.AppointmentDate);
            if (!canBook)
                throw new InvalidOperationException("Service center is at full capacity for the selected date.");

            var appointment = new Appointment
            {
                VehicleId = dto.VehicleId,
                Station_id = dto.Station_id,
                CustomerId = dto.CustomerId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Payment_Pending", // Start with payment pending
                AppointmentPrice = availableServices.Sum(s => s.CustomPrice ?? s.Service.BasePrice ?? 0)
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            foreach (var svc in availableServices)
            {
                _db.AppointmentServices.Add(new Models.AppointmentService
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = svc.ServiceId,
                    ServicePrice = svc.CustomPrice ?? svc.Service.BasePrice ?? 0
                });
            }

            // Increment the appointment count for this service center and date
            await _dailyLimitService.IncrementAppointmentCountAsync(dto.Station_id, dto.AppointmentDate);

            await _db.SaveChangesAsync();

            var stationName = await _db.ServiceCenters
                .Where(sc => sc.Station_id == dto.Station_id)
                .Select(sc => sc.Station_name)
                .FirstOrDefaultAsync() ?? "Unknown";

            return new AppointmentSummaryForCustomerDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                StationName = stationName
            };
        }

        public async Task<AppointmentDetailForAdminDTO> GetAppointmentDetailForAdminAsync(int stationId, int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Vehicle).ThenInclude(v => v.Customer)
                .Include(a => a.AppointmentServices).ThenInclude(asv => asv.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.Station_id == stationId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found for the specified service center.");

            return new AppointmentDetailForAdminDTO
            {
                AppointmentId = appointment.AppointmentId,
                LicensePlate = appointment.Vehicle?.RegistrationNumber ?? "Unknown",
                VehicleType = appointment.Vehicle?.Model ?? "Unknown",
                OwnerName = $"{appointment.Vehicle?.Customer?.FirstName ?? "Unknown"} {appointment.Vehicle?.Customer?.LastName ?? ""}".Trim(),
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                Services = appointment.AppointmentServices?
                    .Where(s => s.Service != null)
                    .Select(s => s.Service.ServiceName)
                    .ToList() ?? new List<string>(),
                VehicleId = appointment.Vehicle?.VehicleId ?? 0,
                ServiceCenterId = appointment.Station_id,
                ServiceCenterName = appointment.ServiceCenter?.Station_name ?? "Unknown"
            };
        }

        public async Task<AppointmentDetailForAdminVehicleDTO> GetAdminAppointmentDetailsByVehicleAsync(int stationId, int customerId, int vehicleId, int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.ServiceCenter)
                .Include(a => a.Vehicle)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asv => asv.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.Station_id == stationId &&
                    a.CustomerId == customerId &&
                    a.VehicleId == vehicleId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found for the specified vehicle, customer, and station.");

            return new AppointmentDetailForAdminVehicleDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                StationName = appointment.ServiceCenter.Station_name ?? "Unknown",
                Services = appointment.AppointmentServices
                            .Select(s => s.Service.ServiceName)
                            .ToList(),
                Notes = appointment.Description
                //BookingFee = appointment.AppointmentPrice ?? 0
            };
        }

        public async Task<object> CleanupDuplicateAppointmentsAsync()
        {
            // Get all appointments grouped by customer, vehicle, and date
            var duplicateGroups = await _db.Appointments
                .GroupBy(a => new { a.CustomerId, a.VehicleId, AppointmentDate = a.AppointmentDate.HasValue ? a.AppointmentDate.Value.Date : (DateTime?)null })
                .Where(g => g.Count() > 1)
                .ToListAsync();

            int totalRemoved = 0;

            foreach (var group in duplicateGroups)
            {
                // Keep the latest appointment (highest ID) and remove the rest
                var appointmentsToRemove = group.OrderByDescending(a => a.AppointmentId).Skip(1).ToList();
                
                foreach (var appointment in appointmentsToRemove)
                {
                    // Remove associated appointment services first
                    var appointmentServices = await _db.AppointmentServices
                        .Where(asv => asv.AppointmentId == appointment.AppointmentId)
                        .ToListAsync();
                    _db.AppointmentServices.RemoveRange(appointmentServices);
                    
                    // Remove the appointment
                    _db.Appointments.Remove(appointment);
                    totalRemoved++;
                }
            }

            await _db.SaveChangesAsync();

            return new { 
                message = $"Cleanup completed. Removed {totalRemoved} duplicate appointments.",
                totalRemoved = totalRemoved
            };
        }

        public async Task<object> GetAppointmentStatisticsAsync()
        {
            var totalAppointments = await _db.Appointments.CountAsync();
            var pendingAppointments = await _db.Appointments.CountAsync(a => a.Status == "Pending");
            var confirmedAppointments = await _db.Appointments.CountAsync(a => a.Status == "Confirmed");
            var paymentPendingAppointments = await _db.Appointments.CountAsync(a => a.Status == "Payment_Pending");

            // Count duplicates
            var duplicateCount = await _db.Appointments
                .GroupBy(a => new { a.CustomerId, a.VehicleId, AppointmentDate = a.AppointmentDate.HasValue ? a.AppointmentDate.Value.Date : (DateTime?)null })
                .Where(g => g.Count() > 1)
                .CountAsync();

            return new
            {
                totalAppointments,
                pendingAppointments,
                confirmedAppointments,
                paymentPendingAppointments,
                duplicateGroups = duplicateCount
            };
        }

        public async Task<object> CompleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.ServiceCenter)
                .Include(a => a.Vehicle)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found.");

            if (appointment.Status == "Completed")
                throw new InvalidOperationException("Appointment is already completed.");

            appointment.Status = "Completed";
            await _db.SaveChangesAsync();

            // Send notification to customer about appointment completion
            try
            {
                var message = $"Your appointment for {appointment.Vehicle?.RegistrationNumber} at {appointment.ServiceCenter?.Station_name} has been completed successfully. Thank you for choosing our service!";
                await _notificationService.CreateAppointmentNotificationAsync(appointment, message);
                _logger.LogInformation("Successfully sent completion notification for appointment {AppointmentId} to customer {CustomerId}", appointmentId, appointment.CustomerId);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the appointment completion
                _logger.LogError(ex, "Error sending completion notification for appointment {AppointmentId}", appointmentId);
            }

            return new
            {
                message = "Appointment completed successfully.",
                appointmentId = appointment.AppointmentId,
                status = appointment.Status,
                completedAt = DateTime.UtcNow
            };
        }
    }
}
