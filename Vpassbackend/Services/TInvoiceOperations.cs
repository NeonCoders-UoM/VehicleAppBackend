using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class TInvoiceOperations
    {
        private readonly ApplicationDbContext _context;

        public TInvoiceOperations(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceDetailsDTO?> GetInvoiceDetailsByAppointmentId(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vehicle)
                .ThenInclude(v => v.Customer)
                .Include(a => a.Service)
                .Include(a => a.ServiceCenter)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return null;

            var invoice = await _context.Invoices
                .Include(i => i.PaymentLogs)
                .FirstOrDefaultAsync(i => i.VehicleId == appointment.VehicleId);

            var services = await _context.Appointments
                .Where(a => a.VehicleId == appointment.VehicleId &&
                            a.ServiceCenter.Station_id == appointment.Station_id)
                .Include(a => a.Service)
                .Select(a => new ServiceCostDTO
                {
                    ServiceName = a.Service.ServiceName,
                    Cost = a.AppointmentPrice ?? a.Service.BasePrice
                })
                .ToListAsync();

            return new InvoiceDetailsDTO
            {
                RegistrationNumber = appointment.Vehicle.RegistrationNumber,
                AppointmentDate = appointment.AppointmentDate,
                LoyaltyPoints = appointment.Customer.LoyaltyPoints,
                ServiceCenterId = appointment.ServiceCenter.Station_id,
                Address = appointment.ServiceCenter.Address,
                TotalCost = services.Sum(s => s.Cost ?? 0),
                PaymentStatus = invoice?.PaymentLogs.LastOrDefault()?.Status ?? "Unpaid",
                Services = services
            };
        }
    }
}
