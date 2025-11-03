using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;

namespace Vpassbackend.Services
{
    public class TInvoiceOperations
    {
        private readonly ApplicationDbContext _context;

        public TInvoiceOperations(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceDetailsDTO?> GetInvoiceDetailsAsync(int customerId, int vehicleId, int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(a => a.ServiceCenter)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asv => asv.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.VehicleId == vehicleId &&
                    a.CustomerId == customerId
                );

            if (appointment == null)
                return null;

            var invoice = await _context.Invoices
                .Include(i => i.PaymentLogs)
                .FirstOrDefaultAsync(i => i.VehicleId == vehicleId);

            var serviceIds = appointment.AppointmentServices
                .Select(s => s.ServiceId)
                .ToList();

            // Get all matching ServiceCenterService entries in one query
            var serviceCenterServices = await _context.ServiceCenterServices
                .Where(scs => scs.Station_id == appointment.Station_id && serviceIds.Contains(scs.ServiceId))
                .ToDictionaryAsync(scs => scs.ServiceId);

            var services = new List<AppointmentServiceDetailDTO>();
            int totalLoyaltyPoints = 0;

            foreach (var apptService in appointment.AppointmentServices)
            {
                serviceCenterServices.TryGetValue(apptService.ServiceId, out var serviceCenterService);

                var estimatedCost = serviceCenterService?.CustomPrice ?? serviceCenterService?.BasePrice ?? 0;
                var loyaltyPoints = serviceCenterService?.LoyaltyPoints ?? 0;

                services.Add(new AppointmentServiceDetailDTO
                {
                    ServiceName = apptService.Service.ServiceName,
                    EstimatedCost = estimatedCost
                });

                totalLoyaltyPoints += loyaltyPoints;
            }

            return new InvoiceDetailsDTO
            {
                RegistrationNumber = appointment.Vehicle.RegistrationNumber,
                AppointmentDate = appointment.AppointmentDate,
                LoyaltyPoints = totalLoyaltyPoints,
                ServiceCenterId = appointment.ServiceCenter.Station_id,
                Address = appointment.ServiceCenter.Address,
                TotalCost = services.Sum(s => s.EstimatedCost),
                PaymentStatus = invoice?.PaymentLogs.LastOrDefault()?.Status ?? "Unpaid",
                Services = services,
                DistanceInKm = null // To be calculated later
            };
        }
    }
}
