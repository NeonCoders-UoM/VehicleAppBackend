using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly ILoyaltyPointsService _loyaltyPointsService;
        private readonly ILogger<AppointmentService> _logger;
        
        public AppointmentService(ApplicationDbContext db, INotificationService notificationService, ILoyaltyPointsService loyaltyPointsService, ILogger<AppointmentService> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _loyaltyPointsService = loyaltyPointsService;
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

            // Send notification for successful appointment creation
            try
            {
                await _notificationService.CreateAppointmentNotificationAsync(
                    appointment, 
                    $"Your appointment has been successfully scheduled for {appointment.AppointmentDate:MMM dd, yyyy} at {stationName}. We look forward to serving you!"
                );
            }
            catch (Exception ex)
            {
                // Log the notification error but don't fail the appointment creation
                // The appointment was successfully created, so we don't want to roll it back
                // due to a notification failure
                _logger.LogWarning(ex, "Failed to send appointment creation notification for appointment {AppointmentId}", appointment.AppointmentId);
            }

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

        public async Task<bool> CompleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.AppointmentServices)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                
            if (appointment == null)
                return false;
            if (appointment.Status == "Completed")
                return true; // Already completed

            // Calculate total loyalty points from all services in this appointment
            var serviceIds = appointment.AppointmentServices.Select(s => s.ServiceId).ToList();
            var serviceCenterServices = await _db.ServiceCenterServices
                .Where(scs => scs.Station_id == appointment.Station_id && serviceIds.Contains(scs.ServiceId))
                .ToListAsync();

            int totalLoyaltyPoints = serviceCenterServices.Sum(scs => scs.LoyaltyPoints ?? 0);

            // Update appointment status
            appointment.Status = "Completed";
            await _db.SaveChangesAsync();

            // Add loyalty points to customer
            if (totalLoyaltyPoints > 0)
            {
                var pointsAdded = await _loyaltyPointsService.UpdateCustomerLoyaltyPointsAsync(appointment.CustomerId, totalLoyaltyPoints);
                if (pointsAdded)
                {
                    _logger.LogInformation($"Added {totalLoyaltyPoints} loyalty points to customer {appointment.CustomerId} for completed appointment {appointmentId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to add loyalty points to customer {appointment.CustomerId} for appointment {appointmentId}");
                }
            }
            
            try
            {
                await _notificationService.CreateAppointmentNotificationAsync(appointment, "Your service appointment has been completed. Thank you for choosing us!");
            }
            catch (Exception ex)
            {
                // Log the notification error but don't fail the appointment completion
                // The appointment was successfully completed, so we don't want to roll it back
                // due to a notification failure
                _logger.LogWarning(ex, "Failed to send appointment completion notification for appointment {AppointmentId}", appointmentId);
            }
            
            return true;
        }
    }
}
