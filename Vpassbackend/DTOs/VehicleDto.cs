namespace Vpassbackend.DTOs
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public required string RegistrationNumber { get; set; }
        public int CustomerId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public int? Mileage { get; set; }
        public string? Fuel { get; set; }
        public int? Year { get; set; }
        public CustomerDto? Customer { get; set; }
    }

    public class VehicleCreateDto
    {
        public required string RegistrationNumber { get; set; }
        public int CustomerId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public int? Mileage { get; set; }
        public string? Fuel { get; set; }
        public int? Year { get; set; }
    }

    public class VehicleUpdateDto
    {
        public string? RegistrationNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public int? Mileage { get; set; }
        public string? Fuel { get; set; }
        public int? Year { get; set; }
    }
}
