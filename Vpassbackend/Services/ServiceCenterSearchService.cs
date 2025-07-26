using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class ServiceCenterSearchService
    {
        private readonly ApplicationDbContext _context;

        public ServiceCenterSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceCenterSearchResultDTO>> GetAvailableServiceCentersAsync(
            double userLat, 
            double userLng, 
            List<int> serviceIds, 
            DateTime appointmentDate)
        {
            var dateOnly = DateOnly.FromDateTime(appointmentDate);
            var weekNumber = GetWeekNumber(appointmentDate);
            var dayOfWeek = appointmentDate.DayOfWeek.ToString();

            // Get all service centers that offer the required services
            var serviceCenters = await _context.ServiceCenters
                .Where(sc => sc.Station_status != null && 
                           sc.Station_status.ToLower() == "active")
                .ToListAsync();

            var results = new List<ServiceCenterSearchResultDTO>();

            foreach (var serviceCenter in serviceCenters)
            {
                // Check if service center offers all required services
                var offeredServices = await _context.ServiceCenterServices
                    .Where(scs => scs.Station_id == serviceCenter.Station_id && 
                                serviceIds.Contains(scs.ServiceId))
                    .Include(scs => scs.Service)
                    .ToListAsync();

                if (offeredServices.Count != serviceIds.Count)
                    continue; // Service center doesn't offer all required services

                // Check if service center is closed on this day
                var isClosed = await _context.ClosureSchedules
                    .AnyAsync(cs => cs.ServiceCenterId == serviceCenter.Station_id && 
                                   cs.WeekNumber == weekNumber && 
                                   cs.Day == dayOfWeek);

                if (isClosed)
                    continue; // Service center is closed on this day

                // Check daily appointment limit (temporary - will be implemented after migration)
                var maxAppointments = 20; // Default limit
                var currentAppointments = 0; // Will be implemented after migration

                // TODO: Uncomment after migration is applied
                // var dailyLimit = await _context.ServiceCenterDailyLimits
                //     .FirstOrDefaultAsync(dl => dl.Station_id == serviceCenter.Station_id && 
                //                              dl.Date == dateOnly);
                // maxAppointments = dailyLimit?.MaxAppointments ?? 20;
                // currentAppointments = dailyLimit?.CurrentAppointments ?? 0;

                if (currentAppointments >= maxAppointments)
                    continue; // Service center is at capacity

                // Check service availability for this specific day
                var unavailableServices = await _context.ServiceAvailabilities
                    .Where(sa => sa.ServiceCenterId == serviceCenter.Station_id && 
                               sa.WeekNumber == weekNumber && 
                               sa.Day == dayOfWeek && 
                               !sa.IsAvailable)
                    .Select(sa => sa.ServiceId)
                    .ToListAsync();

                var requiredServicesUnavailable = serviceIds
                    .Intersect(unavailableServices)
                    .Any();

                if (requiredServicesUnavailable)
                    continue; // Some required services are unavailable on this day

                // Calculate distance
                var distance = CalculateDistance(userLat, userLng, serviceCenter.Latitude, serviceCenter.Longitude);

                // Only include service centers within 1000km
                if (distance > 1000)
                    continue;

                // Calculate total cost and loyalty points
                var totalCost = offeredServices.Sum(s => s.CustomPrice ?? s.BasePrice ?? s.Service.BasePrice ?? 0);
                var totalLoyaltyPoints = offeredServices.Sum(s => s.LoyaltyPoints ?? 0);

                results.Add(new ServiceCenterSearchResultDTO
                {
                    StationId = serviceCenter.Station_id,
                    StationName = serviceCenter.Station_name ?? "Unknown",
                    Address = serviceCenter.Address ?? "",
                    Latitude = serviceCenter.Latitude,
                    Longitude = serviceCenter.Longitude,
                    Distance = distance,
                    TotalCost = totalCost,
                    LoyaltyPoints = totalLoyaltyPoints,
                    AvailableSlots = maxAppointments - currentAppointments,
                    Services = offeredServices.Select(s => new ServiceDetailDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.Service.ServiceName,
                        Cost = s.CustomPrice ?? s.BasePrice ?? s.Service.BasePrice ?? 0
                    }).ToList()
                });
            }

            // Sort by distance
            return results.OrderBy(r => r.Distance).ToList();
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) *
                    Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private int GetWeekNumber(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }

    public class ServiceCenterSearchResultDTO
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = "";
        public string Address { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
        public decimal TotalCost { get; set; }
        public int LoyaltyPoints { get; set; }
        public int AvailableSlots { get; set; }
        public List<ServiceDetailDTO> Services { get; set; } = new();
    }

    public class ServiceDetailDTO
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public decimal Cost { get; set; }
    }
} 