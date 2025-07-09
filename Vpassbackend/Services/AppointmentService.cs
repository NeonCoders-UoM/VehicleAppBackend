using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class AppointmentManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly LocationService _locationService;
        private const double MAX_DISTANCE_KM = 50.0; // Maximum distance in kilometers for nearby service centers
        private const decimal ADVANCE_PAYMENT_PERCENTAGE = 0.1M; // 10% advance payment

        public AppointmentManagementService(ApplicationDbContext context, LocationService locationService)
        {
            _context = context;
            _locationService = locationService;
        }

        // Get nearby service centers that offer the requested services
        public async Task<List<ServiceCenterRecommendationDTO>> FindNearbyServiceCenters(
            double latitude, double longitude, List<int> serviceIds, DateTime appointmentDate)
        {
            // Get all service centers
            var serviceCenters = await _context.ServiceCenters
                .Where(sc => sc.Station_status == "Active")
                .Include(sc => sc.Services)
                .ToListAsync();

            // Filter service centers that have all the requested services
            var validServiceCenters = new List<ServiceCenterRecommendationDTO>();

            foreach (var serviceCenter in serviceCenters)
            {
                // Skip service centers without location data
                if (!serviceCenter.Latitude.HasValue || !serviceCenter.Longitude.HasValue)
                {
                    continue;
                }

                // Calculate distance
                double distance = _locationService.CalculateDistance(
                    latitude, longitude,
                    serviceCenter.Latitude.Value, serviceCenter.Longitude.Value);

                // Skip service centers that are too far away
                if (distance > MAX_DISTANCE_KM)
                {
                    continue;
                }

                // Check if the service center offers all the requested services
                var availableServiceIds = serviceCenter.Services.Select(s => s.ServiceId).ToList();
                bool hasAllServices = serviceIds.All(id => availableServiceIds.Contains(id));

                if (hasAllServices)
                {
                    // Calculate estimated total cost
                    decimal estimatedTotalCost = 0;
                    var serviceDetailsList = new List<ServiceDetailsDTO>();

                    foreach (var serviceId in serviceIds)
                    {
                        var service = serviceCenter.Services.FirstOrDefault(s => s.ServiceId == serviceId);
                        if (service != null && service.BasePrice.HasValue)
                        {
                            estimatedTotalCost += service.BasePrice.Value;

                            serviceDetailsList.Add(new ServiceDetailsDTO
                            {
                                ServiceId = service.ServiceId,
                                ServiceName = service.ServiceName,
                                Description = service.Description ?? "",
                                Price = service.BasePrice.Value
                            });
                        }
                    }

                    // Calculate advance payment amount (10% of estimated cost)
                    decimal advancePaymentAmount = estimatedTotalCost * ADVANCE_PAYMENT_PERCENTAGE;

                    validServiceCenters.Add(new ServiceCenterRecommendationDTO
                    {
                        ServiceCenterId = serviceCenter.Station_id,
                        Name = serviceCenter.Station_name ?? "Unknown Service Center",
                        Address = serviceCenter.Address ?? "No Address",
                        Distance = Math.Round(distance, 2),
                        EstimatedTotalCost = estimatedTotalCost,
                        AdvancePaymentAmount = advancePaymentAmount,
                        AvailableServices = serviceDetailsList
                    });
                }
            }

            // Sort by distance
            return validServiceCenters.OrderBy(sc => sc.Distance).ToList();
        }

        // Book an appointment
        public async Task<AppointmentConfirmationDTO> CreateAppointment(AppointmentBookingDTO bookingDto)
        {
            // Check if vehicle and customer exist
            var vehicle = await _context.Vehicles.FindAsync(bookingDto.VehicleId);
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

            var customer = await _context.Customers.FindAsync(bookingDto.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            var serviceCenter = await _context.ServiceCenters.FindAsync(bookingDto.ServiceCenterId);
            if (serviceCenter == null)
            {
                throw new Exception("Service center not found");
            }

            // Calculate estimated total cost
            decimal estimatedTotalCost = 0;
            var services = await _context.Services
                .Where(s => bookingDto.ServiceIds.Contains(s.ServiceId) && s.Station_id == bookingDto.ServiceCenterId)
                .ToListAsync();

            if (services.Count != bookingDto.ServiceIds.Count)
            {
                throw new Exception("One or more selected services are not available at this service center");
            }

            foreach (var service in services)
            {
                if (service.BasePrice.HasValue)
                {
                    estimatedTotalCost += service.BasePrice.Value;
                }
            }

            // Calculate advance payment amount (10%)
            decimal advancePaymentAmount = estimatedTotalCost * ADVANCE_PAYMENT_PERCENTAGE;

            // Create new appointment
            var appointment = new Appointment
            {
                VehicleId = bookingDto.VehicleId,
                CustomerId = bookingDto.CustomerId,
                ServiceCenterId = bookingDto.ServiceCenterId,
                AppointmentDate = bookingDto.AppointmentDate,
                Description = bookingDto.Description,
                Status = "Pending",
                EstimatedTotalCost = estimatedTotalCost,
                AdvancePaymentAmount = advancePaymentAmount,
                IsAdvancePaymentCompleted = false
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Add services to the appointment
            foreach (var service in services)
            {
                var appointmentService = new AppointmentService
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = service.ServiceId,
                    ServicePrice = service.BasePrice ?? 0
                };

                _context.AppointmentServices.Add(appointmentService);
            }

            await _context.SaveChangesAsync();

            return new AppointmentConfirmationDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                ServiceCenterName = serviceCenter.Station_name ?? "Unknown Service Center",
                ServiceCenterAddress = serviceCenter.Address ?? "No Address",
                TotalCost = estimatedTotalCost,
                AdvancePayment = advancePaymentAmount,
                PaymentStatus = "Pending"
            };
        }

        // Process payment for an appointment
        public async Task<AppointmentConfirmationDTO> HandlePayment(AppointmentPaymentDTO paymentDto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.ServiceCenter)
                .FirstOrDefaultAsync(a => a.AppointmentId == paymentDto.AppointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            if (appointment.IsAdvancePaymentCompleted)
            {
                throw new Exception("Payment for this appointment has already been completed");
            }

            // Verify payment amount
            if (paymentDto.Amount < appointment.AdvancePaymentAmount)
            {
                throw new Exception("Payment amount is less than required advance payment");
            }

            // Create payment log
            var paymentLog = new PaymentLog
            {
                AppointmentId = appointment.AppointmentId,
                Amount = paymentDto.Amount,
                Status = "Completed",
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentDto.PaymentMethod,
                TransactionReference = paymentDto.TransactionReference,
                PaymentType = "Advance"
            };

            _context.PaymentLogs.Add(paymentLog);

            // Update appointment status
            appointment.IsAdvancePaymentCompleted = true;
            appointment.Status = "Confirmed";

            await _context.SaveChangesAsync();

            return new AppointmentConfirmationDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                ServiceCenterName = appointment.ServiceCenter.Station_name ?? "Unknown Service Center",
                ServiceCenterAddress = appointment.ServiceCenter.Address ?? "No Address",
                TotalCost = appointment.EstimatedTotalCost,
                AdvancePayment = appointment.AdvancePaymentAmount,
                PaymentStatus = "Completed"
            };
        }

        // Get appointment details
        public async Task<AppointmentConfirmationDTO> FetchAppointmentDetails(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.ServiceCenter)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }

            return new AppointmentConfirmationDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                ServiceCenterName = appointment.ServiceCenter.Station_name ?? "Unknown Service Center",
                ServiceCenterAddress = appointment.ServiceCenter.Address ?? "No Address",
                TotalCost = appointment.EstimatedTotalCost,
                AdvancePayment = appointment.AdvancePaymentAmount,
                PaymentStatus = appointment.IsAdvancePaymentCompleted ? "Completed" : "Pending"
            };
        }
    }
}
