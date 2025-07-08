namespace Vpassbackend.DTOs
{
    public class ServiceCenterDto
    {
        public int ServiceId { get; set; }
        public string OwnerName { get; set; }
        public string? VATNumber { get; set; }
        public string? RegisterationNumber { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Address { get; set; }
        public string? AccountStatus { get; set; }
    }

    public class ServiceCenterCreateDto
    {
        public string OwnerName { get; set; }
        public string? VATNumber { get; set; }
        public string? RegisterationNumber { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Address { get; set; }
    }

    public class ServiceCenterUpdateDto
    {
        public string? OwnerName { get; set; }
        public string? VATNumber { get; set; }
        public string? RegisterationNumber { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Address { get; set; }
        public string? AccountStatus { get; set; }
    }
}
