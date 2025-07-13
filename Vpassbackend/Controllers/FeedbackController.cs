using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Feedback
        [HttpPost]
        public async Task<IActionResult> CreateFeedback(CreateFeedbackDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify that the customer, service center, and vehicle exist
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            var serviceCenter = await _context.ServiceCenters.FindAsync(dto.ServiceCenterId);
            if (serviceCenter == null)
            {
                return BadRequest("Service center not found");
            }

            var vehicle = await _context.Vehicles.FindAsync(dto.VehicleId);
            if (vehicle == null)
            {
                return BadRequest("Vehicle not found");
            }

            // Verify that the vehicle belongs to the customer
            if (vehicle.CustomerId != dto.CustomerId)
            {
                return BadRequest("Vehicle does not belong to the specified customer");
            }

            var feedback = new Feedback
            {
                CustomerId = dto.CustomerId,
                ServiceCenterId = dto.ServiceCenterId,
                VehicleId = dto.VehicleId,
                Rating = dto.Rating,
                Comments = dto.Comments,
                ServiceDate = dto.ServiceDate,
                FeedbackDate = DateTime.UtcNow,
                Customer = customer,
                ServiceCenter = serviceCenter,
                Vehicle = vehicle
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.FeedbackId }, 
                new FeedbackDTO
                {
                    FeedbackId = feedback.FeedbackId,
                    CustomerId = feedback.CustomerId,
                    ServiceCenterId = feedback.ServiceCenterId,
                    VehicleId = feedback.VehicleId,
                    Rating = feedback.Rating,
                    Comments = feedback.Comments,
                    ServiceDate = feedback.ServiceDate,
                    FeedbackDate = feedback.FeedbackDate,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    ServiceCenterName = serviceCenter.Station_name,
                    VehicleRegistrationNumber = vehicle.RegistrationNumber
                });
        }

        // GET: api/Feedback
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,ServiceCenterAdmin")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetAllFeedbacks(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? serviceCenterId = null,
            [FromQuery] int? minRating = null,
            [FromQuery] int? maxRating = null)
        {
            var query = _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.ServiceCenter)
                .Include(f => f.Vehicle)
                .AsQueryable();

            // Apply filters
            if (serviceCenterId.HasValue)
            {
                query = query.Where(f => f.ServiceCenterId == serviceCenterId.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(f => f.Rating >= minRating.Value);
            }

            if (maxRating.HasValue)
            {
                query = query.Where(f => f.Rating <= maxRating.Value);
            }

            // Order by feedback date (newest first)
            query = query.OrderByDescending(f => f.FeedbackDate);

            // Apply pagination
            var feedbacks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackDTO
                {
                    FeedbackId = f.FeedbackId,
                    CustomerId = f.CustomerId,
                    ServiceCenterId = f.ServiceCenterId,
                    VehicleId = f.VehicleId,
                    Rating = f.Rating,
                    Comments = f.Comments,
                    ServiceDate = f.ServiceDate,
                    FeedbackDate = f.FeedbackDate,
                    CustomerName = $"{f.Customer.FirstName} {f.Customer.LastName}",
                    ServiceCenterName = f.ServiceCenter.Station_name,
                    VehicleRegistrationNumber = f.Vehicle.RegistrationNumber
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        // GET: api/Feedback/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackDTO>> GetFeedbackById(int id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.ServiceCenter)
                .Include(f => f.Vehicle)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            var feedbackDto = new FeedbackDTO
            {
                FeedbackId = feedback.FeedbackId,
                CustomerId = feedback.CustomerId,
                ServiceCenterId = feedback.ServiceCenterId,
                VehicleId = feedback.VehicleId,
                Rating = feedback.Rating,
                Comments = feedback.Comments,
                ServiceDate = feedback.ServiceDate,
                FeedbackDate = feedback.FeedbackDate,
                CustomerName = $"{feedback.Customer.FirstName} {feedback.Customer.LastName}",
                ServiceCenterName = feedback.ServiceCenter.Station_name,
                VehicleRegistrationNumber = feedback.Vehicle.RegistrationNumber
            };

            return Ok(feedbackDto);
        }

        // GET: api/Feedback/Customer/{customerId}
        [HttpGet("Customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetCustomerFeedbacks(int customerId)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.ServiceCenter)
                .Include(f => f.Vehicle)
                .Where(f => f.CustomerId == customerId)
                .OrderByDescending(f => f.FeedbackDate)
                .Select(f => new FeedbackDTO
                {
                    FeedbackId = f.FeedbackId,
                    CustomerId = f.CustomerId,
                    ServiceCenterId = f.ServiceCenterId,
                    VehicleId = f.VehicleId,
                    Rating = f.Rating,
                    Comments = f.Comments,
                    ServiceDate = f.ServiceDate,
                    FeedbackDate = f.FeedbackDate,
                    CustomerName = $"{f.Customer.FirstName} {f.Customer.LastName}",
                    ServiceCenterName = f.ServiceCenter.Station_name,
                    VehicleRegistrationNumber = f.Vehicle.RegistrationNumber
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        // GET: api/Feedback/ServiceCenter/{serviceCenterId}
        [HttpGet("ServiceCenter/{serviceCenterId}")]
        [Authorize(Roles = "Admin,SuperAdmin,ServiceCenterAdmin")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetServiceCenterFeedbacks(int serviceCenterId)
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Customer)
                .Include(f => f.ServiceCenter)
                .Include(f => f.Vehicle)
                .Where(f => f.ServiceCenterId == serviceCenterId)
                .OrderByDescending(f => f.FeedbackDate)
                .Select(f => new FeedbackDTO
                {
                    FeedbackId = f.FeedbackId,
                    CustomerId = f.CustomerId,
                    ServiceCenterId = f.ServiceCenterId,
                    VehicleId = f.VehicleId,
                    Rating = f.Rating,
                    Comments = f.Comments,
                    ServiceDate = f.ServiceDate,
                    FeedbackDate = f.FeedbackDate,
                    CustomerName = $"{f.Customer.FirstName} {f.Customer.LastName}",
                    ServiceCenterName = f.ServiceCenter.Station_name,
                    VehicleRegistrationNumber = f.Vehicle.RegistrationNumber
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        // GET: api/Feedback/Stats
        [HttpGet("Stats")]
        [Authorize(Roles = "Admin,SuperAdmin,ServiceCenterAdmin")]
        public async Task<ActionResult<FeedbackStatsDTO>> GetFeedbackStats([FromQuery] int? serviceCenterId = null)
        {
            var query = _context.Feedbacks.AsQueryable();

            if (serviceCenterId.HasValue)
            {
                query = query.Where(f => f.ServiceCenterId == serviceCenterId.Value);
            }

            var feedbacks = await query.ToListAsync();

            if (!feedbacks.Any())
            {
                return Ok(new FeedbackStatsDTO
                {
                    AverageRating = 0,
                    TotalFeedbacks = 0,
                    RatingCounts = new Dictionary<int, int>()
                });
            }

            var stats = new FeedbackStatsDTO
            {
                AverageRating = Math.Round(feedbacks.Average(f => f.Rating), 1),
                TotalFeedbacks = feedbacks.Count,
                RatingCounts = feedbacks
                    .GroupBy(f => f.Rating)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Ensure all rating levels (1-5) are represented
            for (int i = 1; i <= 5; i++)
            {
                if (!stats.RatingCounts.ContainsKey(i))
                {
                    stats.RatingCounts[i] = 0;
                }
            }

            return Ok(stats);
        }

        // PUT: api/Feedback/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, UpdateFeedbackDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            // Update only provided fields
            if (dto.Rating.HasValue)
            {
                feedback.Rating = dto.Rating.Value;
            }

            if (dto.Comments != null)
            {
                feedback.Comments = dto.Comments;
            }

            if (dto.ServiceDate != null)
            {
                feedback.ServiceDate = dto.ServiceDate;
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/Feedback/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }
    }
}
