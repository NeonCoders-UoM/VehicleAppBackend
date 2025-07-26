using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public class DailyLimitService
    {
        private readonly ApplicationDbContext _context;

        public DailyLimitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanBookAppointmentAsync(int stationId, DateTime appointmentDate)
        {
            var dateOnly = DateOnly.FromDateTime(appointmentDate);
            
            var dailyLimit = await _context.ServiceCenterDailyLimits
                .FirstOrDefaultAsync(dl => dl.Station_id == stationId && dl.Date == dateOnly);

            if (dailyLimit == null)
            {
                // Create a new daily limit if it doesn't exist
                dailyLimit = new ServiceCenterDailyLimit
                {
                    Station_id = stationId,
                    Date = dateOnly,
                    MaxAppointments = 20, // Default limit
                    CurrentAppointments = 0
                };
                _context.ServiceCenterDailyLimits.Add(dailyLimit);
                await _context.SaveChangesAsync();
            }

            return dailyLimit.CurrentAppointments < dailyLimit.MaxAppointments;
        }

        public async Task IncrementAppointmentCountAsync(int stationId, DateTime appointmentDate)
        {
            var dateOnly = DateOnly.FromDateTime(appointmentDate);
            
            var dailyLimit = await _context.ServiceCenterDailyLimits
                .FirstOrDefaultAsync(dl => dl.Station_id == stationId && dl.Date == dateOnly);

            if (dailyLimit == null)
            {
                // Create a new daily limit if it doesn't exist
                dailyLimit = new ServiceCenterDailyLimit
                {
                    Station_id = stationId,
                    Date = dateOnly,
                    MaxAppointments = 20, // Default limit
                    CurrentAppointments = 0
                };
                _context.ServiceCenterDailyLimits.Add(dailyLimit);
            }

            dailyLimit.CurrentAppointments++;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementAppointmentCountAsync(int stationId, DateTime appointmentDate)
        {
            var dateOnly = DateOnly.FromDateTime(appointmentDate);
            
            var dailyLimit = await _context.ServiceCenterDailyLimits
                .FirstOrDefaultAsync(dl => dl.Station_id == stationId && dl.Date == dateOnly);

            if (dailyLimit != null && dailyLimit.CurrentAppointments > 0)
            {
                dailyLimit.CurrentAppointments--;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetDailyLimitAsync(int stationId, DateTime date, int maxAppointments)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            
            var dailyLimit = await _context.ServiceCenterDailyLimits
                .FirstOrDefaultAsync(dl => dl.Station_id == stationId && dl.Date == dateOnly);

            if (dailyLimit == null)
            {
                dailyLimit = new ServiceCenterDailyLimit
                {
                    Station_id = stationId,
                    Date = dateOnly,
                    MaxAppointments = maxAppointments,
                    CurrentAppointments = 0
                };
                _context.ServiceCenterDailyLimits.Add(dailyLimit);
            }
            else
            {
                dailyLimit.MaxAppointments = maxAppointments;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetAvailableSlotsAsync(int stationId, DateTime appointmentDate)
        {
            var dateOnly = DateOnly.FromDateTime(appointmentDate);
            
            var dailyLimit = await _context.ServiceCenterDailyLimits
                .FirstOrDefaultAsync(dl => dl.Station_id == stationId && dl.Date == dateOnly);

            if (dailyLimit == null)
            {
                return 20; // Default available slots
            }

            return Math.Max(0, dailyLimit.MaxAppointments - dailyLimit.CurrentAppointments);
        }
    }
} 