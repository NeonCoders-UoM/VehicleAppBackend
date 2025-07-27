namespace Vpassbackend.DTOs
{
    public class CustomerVehicleDTO
    {
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string Client { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Fuel { get; set; }
        public int? Year { get; set; }
        public string RegistrationNumber { get; set; }
    }
}
