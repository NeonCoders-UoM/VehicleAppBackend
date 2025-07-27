using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class AppointmentPaymentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdvancePaymentCalculationDTO> CalculateAdvancePaymentAsync(int appointmentId, int customerId, int vehicleId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vehicle)
                .Include(a => a.ServiceCenter)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asv => asv.Service)
                .FirstOrDefaultAsync(a =>
                    a.AppointmentId == appointmentId &&
                    a.CustomerId == customerId &&
                    a.VehicleId == vehicleId);

            if (appointment == null)
                throw new KeyNotFoundException("Appointment not found");

            var serviceIds = appointment.AppointmentServices.Select(s => s.ServiceId).ToList();

            // Get accurate pricing from ServiceCenterService
            var serviceCenterServices = await _context.ServiceCenterServices
                .Where(scs => scs.Station_id == appointment.Station_id && serviceIds.Contains(scs.ServiceId))
                .Include(scs => scs.Service)
                .ToListAsync();

            var services = serviceCenterServices.Select(scs => new AppointmentServiceDetailDTO
            {
                ServiceName = scs.Service.ServiceName,
                EstimatedCost = scs.CustomPrice ?? scs.BasePrice ?? scs.Service.BasePrice ?? 0
            }).ToList();

            var totalCost = services.Sum(s => s.EstimatedCost);
            var advancePaymentAmount = CalculateAdvancePaymentAmount(totalCost);
            var remainingAmount = totalCost - advancePaymentAmount;

            return new AdvancePaymentCalculationDTO
            {
                AppointmentId = appointmentId,
                TotalCost = totalCost,
                AdvancePaymentAmount = advancePaymentAmount,
                RemainingAmount = remainingAmount,
                PaymentType = "Advance",
                ServiceCenterName = appointment.ServiceCenter.Station_name,
                VehicleRegistration = appointment.Vehicle.RegistrationNumber,
                AppointmentDate = appointment.AppointmentDate ?? DateTime.MinValue,
                Services = services
            };
        }

        private decimal CalculateAdvancePaymentAmount(decimal totalCost)
        {
            if (totalCost < 10000)
            {
                return Math.Round(totalCost * 0.10m, 2); // 10% of total cost
            }
            else
            {
                return 1000m; // Fixed Rs. 1000 for costs >= Rs. 10,000
            }
        }

        public async Task<AppointmentPaymentDTO> CreateAppointmentPaymentAsync(AdvancePaymentRequestDTO request)
        {
            var calculation = await CalculateAdvancePaymentAsync(request.AppointmentId, request.CustomerId, request.VehicleId);

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException("Vehicle not found");

            // Create Invoice for appointment payment
            var invoice = new Invoice
            {
                VehicleId = request.VehicleId,
                Vehicle = vehicle, // Set navigation property
                TotalCost = calculation.AdvancePaymentAmount,
                InvoiceDate = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Create PaymentLog for tracking
            var paymentLog = new PaymentLog
            {
                InvoiceId = invoice.InvoiceId,
                PaymentDate = null,
                Status = "Pending"
            };
            _context.PaymentLogs.Add(paymentLog);
            await _context.SaveChangesAsync();

            // Update appointment status to indicate payment is pending
            var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
            if (appointment != null)
            {
                appointment.Status = "Payment_Pending";
                await _context.SaveChangesAsync();
            }

            return new AppointmentPaymentDTO
            {
                AppointmentId = request.AppointmentId,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                TotalCost = calculation.TotalCost,
                AdvancePaymentAmount = calculation.AdvancePaymentAmount,
                RemainingAmount = calculation.RemainingAmount,
                PaymentStatus = "Pending",
                PaymentDate = null,
                OrderId = $"appointment_{request.AppointmentId}_{invoice.InvoiceId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            };
        }

        public async Task<bool> UpdatePaymentStatusAsync(string orderId, string status)
        {
            Console.WriteLine($"UpdatePaymentStatusAsync called with orderId: {orderId}, status: {status}");
            
            // Parse orderId to get invoiceId
            if (!orderId.StartsWith("appointment_"))
            {
                Console.WriteLine($"Invalid orderId format: {orderId}");
                return false;
            }

            var parts = orderId.Split('_');
            if (parts.Length < 3 || !int.TryParse(parts[2], out int invoiceId))
            {
                Console.WriteLine($"Failed to parse invoiceId from orderId: {orderId}");
                return false;
            }

            Console.WriteLine($"Parsed invoiceId: {invoiceId}");

            var paymentLog = await _context.PaymentLogs
                .Include(p => p.Invoice)
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.LogId)
                .FirstOrDefaultAsync();

            if (paymentLog == null)
            {
                Console.WriteLine($"PaymentLog not found for invoiceId: {invoiceId}");
                return false;
            }

            paymentLog.Status = status;
            paymentLog.PaymentDate = status == "Paid" ? DateTime.UtcNow : null;

            // Update appointment status if payment is successful
            if (status == "Paid")
            {
                var appointmentId = int.Parse(parts[1]);
                Console.WriteLine($"Updating appointment {appointmentId} status to Confirmed");
                
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                if (appointment != null)
                {
                    appointment.Status = "Confirmed";
                    Console.WriteLine($"Appointment {appointmentId} status updated to Confirmed");
                }
                else
                {
                    Console.WriteLine($"Appointment {appointmentId} not found");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Payment status update completed successfully");
            return true;
        }

        public async Task<AppointmentPaymentDTO?> GetPaymentStatusAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return null;

            var invoice = await _context.Invoices
                .Include(i => i.PaymentLogs)
                .Where(i => i.VehicleId == appointment.VehicleId)
                .OrderByDescending(i => i.InvoiceDate)
                .FirstOrDefaultAsync();

            if (invoice == null)
                return null;

            var latestPayment = invoice.PaymentLogs
                .OrderByDescending(p => p.LogId)
                .FirstOrDefault();

            var calculation = await CalculateAdvancePaymentAsync(appointmentId, appointment.CustomerId, appointment.VehicleId);

            return new AppointmentPaymentDTO
            {
                AppointmentId = appointmentId,
                CustomerId = appointment.CustomerId,
                VehicleId = appointment.VehicleId,
                TotalCost = calculation.TotalCost,
                AdvancePaymentAmount = calculation.AdvancePaymentAmount,
                RemainingAmount = calculation.RemainingAmount,
                PaymentStatus = latestPayment?.Status ?? "Unknown",
                PaymentDate = latestPayment?.PaymentDate,
                OrderId = $"appointment_{appointmentId}_{invoice.InvoiceId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
            };
        }
    }
} 