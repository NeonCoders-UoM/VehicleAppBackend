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
            var scServices = await _db.ServiceCenterServices
                .Where(s => s.Station_id == dto.Station_id && dto.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            var appointments = new List<Appointment>();

            foreach (var sid in dto.ServiceIds)
            {
                var scService = scServices.FirstOrDefault(s => s.ServiceId == sid);

                if (scService == null)
                    throw new Exception($"Service ID {sid} not available at station {dto.Station_id}");

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

        public async Task<List<AppointmentSummaryForCustomerDTO>> GetCustomerAppointmentsAsync(int customerId)
        {
            return await _db.Appointments
                .Include(a => a.ServiceCenter)
                .Where(a => a.CustomerId == customerId)
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new AppointmentSummaryForCustomerDTO
                {
                    AppointmentId = g.First().AppointmentId,
                    StationName = g.First().ServiceCenter.Station_name,
                    AppointmentDate = g.Key ?? DateTime.MinValue
                }).ToListAsync();
        }

        public async Task<AppointmentDetailForCustomerDTO> GetCustomerAppointmentDetailsAsync(int appointmentId)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Service)
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceCenter)
                .Where(a => a.AppointmentId == appointmentId)
                .ToListAsync();

            var first = appointments.FirstOrDefault();
            if (first == null)
                throw new Exception("Appointment not found");

            return new AppointmentDetailForCustomerDTO
            {
                VehicleRegistration = first.Vehicle.RegistrationNumber,
                AppointmentDate = first.AppointmentDate ?? DateTime.MinValue,
                ServiceCenterId = first.ServiceCenter.Station_id,
                ServiceCenterAddress = first.ServiceCenter.Address,
                Services = appointments.Select(a => a.Service.ServiceName).ToList()
            };
        }

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

        public async Task UpdateAppointmentStatusAsync(int id, string status)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) throw new Exception("Appointment not found");
            appointment.Status = status;
            await _db.SaveChangesAsync();
        }
    }
}
