using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByServiceIdAsync(int serviceId);
        Task<AppointmentDto> CreateAppointmentAsync(AppointmentCreateDto appointmentCreateDto);
        Task<AppointmentDto?> UpdateAppointmentAsync(int id, AppointmentUpdateDto appointmentUpdateDto);
        Task<bool> DeleteAppointmentAsync(int id);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AppointmentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
            
            if (appointment == null)
            {
                return null;
            }

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCustomerIdAsync(int customerId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByVehicleIdAsync(int vehicleId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.VehicleId == vehicleId)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByServiceIdAsync(int serviceId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.ServiceId == serviceId)
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                .Include(a => a.Service)
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(AppointmentCreateDto appointmentCreateDto)
        {
            var appointment = _mapper.Map<Appointment>(appointmentCreateDto);
            appointment.Status = "Scheduled"; // Default status for new appointments
            
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<AppointmentDto?> UpdateAppointmentAsync(int id, AppointmentUpdateDto appointmentUpdateDto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return null;
            }

            _mapper.Map(appointmentUpdateDto, appointment);
            await _context.SaveChangesAsync();

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return false;
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
