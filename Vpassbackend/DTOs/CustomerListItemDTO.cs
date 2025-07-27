namespace Vpassbackend.DTOs
{
    public class CustomerListItemDTO
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int LoyaltyPoints { get; set; }
        public string NIC { get; set; }
    }
}
