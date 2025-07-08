namespace Vpassbackend.DTOs
{
    public class BorderPointDto
    {
        public int PointId { get; set; }
        public int VehicleId { get; set; }
        public string? CheckPoint { get; set; }
        public DateTime? CheckDate { get; set; }
        public string? EntryPoint { get; set; }
    }

    public class BorderPointCreateDto
    {
        public int VehicleId { get; set; }
        public string? CheckPoint { get; set; }
        public DateTime CheckDate { get; set; }
        public string? EntryPoint { get; set; }
    }

    public class BorderPointUpdateDto
    {
        public string? CheckPoint { get; set; }
        public DateTime? CheckDate { get; set; }
        public string? EntryPoint { get; set; }
    }
}
