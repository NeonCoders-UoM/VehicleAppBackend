namespace Vpassbackend.DTOs
{
    public class VehicleRegistrationDto
    {
        public required string RegistrationNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public int? Mileage { get; set; }
        public string? Fuel { get; set; }
        public int? Year { get; set; }
    }
}
