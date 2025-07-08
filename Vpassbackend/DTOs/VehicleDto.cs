namespace Vpassbackend.DTOs
{
    public class VehicleDto
    {
        public int VehicleId { get; set; }
        public string RegistrationNumber { get; set; }
        public int CustomerId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public string? EngineNumber { get; set; }
        public int? Year { get; set; }
        public CustomerDto? Customer { get; set; }
    }

    public class VehicleCreateDto
    {
        public string RegistrationNumber { get; set; }
        public int CustomerId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public string? EngineNumber { get; set; }
        public int? Year { get; set; }
    }

    public class VehicleUpdateDto
    {
        public string? RegistrationNumber { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? ChassisNumber { get; set; }
        public string? EngineNumber { get; set; }
        public int? Year { get; set; }
    }
}
