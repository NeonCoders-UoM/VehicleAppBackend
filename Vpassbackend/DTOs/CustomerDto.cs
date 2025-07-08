namespace Vpassbackend.DTOs
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? NIC { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? LoyaltyPoints { get; set; }
        public bool? ProfileActive { get; set; }
    }

    public class CustomerCreateDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? NIC { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class CustomerUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? NIC { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public bool? ProfileActive { get; set; }
    }
}
