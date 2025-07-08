namespace Vpassbackend.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public int? LoyaltyPoints { get; set; }
        public int Station_id { get; set; }
        public ServiceCenterDto? ServiceCenter { get; set; }
    }

    public class ServiceCreateDto
    {
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public int? LoyaltyPoints { get; set; }
        public int Station_id { get; set; }
    }

    public class ServiceUpdateDto
    {
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public int? LoyaltyPoints { get; set; }
    }
}
