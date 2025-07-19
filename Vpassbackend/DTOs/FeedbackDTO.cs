using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class FeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int CustomerId { get; set; }
        public int ServiceCenterId { get; set; }
        public int VehicleId { get; set; }
        public int Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime FeedbackDate { get; set; }
        public string? ServiceDate { get; set; }

        // Related entity information
        public string? CustomerName { get; set; }
        public string? ServiceCenterName { get; set; }
        public string? VehicleRegistrationNumber { get; set; }
    }

    public class CreateFeedbackDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ServiceCenterId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        [MaxLength(50)]
        public string? ServiceDate { get; set; }
    }

    public class UpdateFeedbackDTO
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        [MaxLength(50)]
        public string? ServiceDate { get; set; }
    }

    public class FeedbackStatsDTO
    {
        public double AverageRating { get; set; }
        public int TotalFeedbacks { get; set; }
        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();
    }
}
