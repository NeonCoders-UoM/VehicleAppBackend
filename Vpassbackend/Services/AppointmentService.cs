using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _db;
        public AppointmentService(ApplicationDbContext db) => _db = db;

        public async Task<List<Appointment>> CreateAppointmentAsync(AppointmentCreateDTO dto)
        {
            // Validate customer exists
            var customerExists = await _db.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
            if (!customerExists)
                throw new KeyNotFoundException("Customer not found.");

            // Validate vehicle belongs to customer
            var vehicleExists = await _db.Vehicles.AnyAsync(v => v.VehicleId == dto.VehicleId && v.CustomerId == dto.CustomerId);
            if (!vehicleExists)
                throw new KeyNotFoundException("Vehicle not found or does not belong to customer.");

            var scServices = await _db.ServiceCenterServices
                .Where(s => s.Station_id == dto.Station_id && dto.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            var appointments = new List<Appointment>();

            foreach (var sid in dto.ServiceIds)
            {
                var scService = scServices.FirstOrDefault(s => s.ServiceId == sid);
                if (scService == null)
                    throw new KeyNotFoundException($"Service ID {sid} not available at station {dto.Station_id}");

                var appointment = new Appointment
                {
                    VehicleId = dto.VehicleId,
                    ServiceId = sid,
                    Station_id = dto.Station_id,
                    CustomerId = dto.CustomerId,
                    AppointmentDate = dto.AppointmentDate,
                    Status = "Pending",
                    Type = dto.Type,
                    Description = dto.Description,
                    AppointmentPrice = scService.CustomPrice
                };

                _db.Appointments.Add(appointment);
                appointments.Add(appointment);
            }

            await _db.SaveChangesAsync();
            return appointments;
        }

        // Get all appointments for a specific vehicle
        public async Task<List<AppointmentSummaryForCustomerDTO>> GetAppointmentsByCustomerVehicleAsync(int customerId, int vehicleId)
        {
            return await _db.Appointments
                .Include(a => a.ServiceCenter)
                .Where(a => a.CustomerId == customerId && a.VehicleId == vehicleId)
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new AppointmentSummaryForCustomerDTO
                {
                    AppointmentId = g.First().AppointmentId,
                    StationName = g.First().ServiceCenter.Station_name,
                    AppointmentDate = g.Key ?? DateTime.MinValue
                }).ToListAsync();
        }

        // Detailed appointment preview
        public async Task<AppointmentDetailForCustomerDTO> GetCustomerAppointmentDetailsAsync(int appointmentId)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Service)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceCenter)
                .Where(a => a.AppointmentId == appointmentId)
                .ToListAsync();

            var first = appointments.First();

            return new AppointmentDetailForCustomerDTO
            {
                VehicleRegistration = first.Vehicle.RegistrationNumber,
                AppointmentDate = first.AppointmentDate ?? DateTime.MinValue,
                ServiceCenterId = first.ServiceCenter.Station_id,
                ServiceCenterAddress = first.ServiceCenter.Address,
                Services = appointments.Select(a => a.Service.ServiceName).ToList()
            };
        }

        // list of appointments per center
        public async Task<List<AppointmentSummaryForAdminDTO>> GetAppointmentsForServiceCenterAsync(int stationId)
        {
            return await _db.Appointments
                .Include(a => a.Customer)
                .Where(a => a.Station_id == stationId)
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new AppointmentSummaryForAdminDTO
                {
                    AppointmentId = g.First().AppointmentId,
                    OwnerName = g.First().Customer.FirstName + " " + g.First().Customer.LastName,
                    AppointmentDate = g.Key ?? DateTime.MinValue
                }).ToListAsync();
        }

        // View full appointment details
        public async Task<AppointmentDetailForAdminDTO> GetAppointmentDetailForAdminAsync(int appointmentId)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Vehicle).ThenInclude(v => v.Customer)
                .Include(a => a.Service)
                .Where(a => a.AppointmentId == appointmentId)
                .ToListAsync();

            var first = appointments.First();

            return new AppointmentDetailForAdminDTO
            {
                AppointmentId = first.AppointmentId,
                LicensePlate = first.Vehicle.RegistrationNumber,
                VehicleType = first.Vehicle.Model,
                OwnerName = first.Vehicle.Customer.FirstName + " " + first.Vehicle.Customer.LastName,
                AppointmentDate = first.AppointmentDate ?? DateTime.MinValue,
                Services = appointments.Select(a => a.Service.ServiceName).ToList()
            };
        }
    }
}
