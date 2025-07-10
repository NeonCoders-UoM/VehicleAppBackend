namespace Vpassbackend.DTOs
{
    public class ServiceCenterResponseDTO
    {
        public int ServiceCenterId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string VATNumber { get; set; } = string.Empty;
        public string RegisterationNumber { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string StationStatus { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int ServiceCount { get; set; }
    }
}
