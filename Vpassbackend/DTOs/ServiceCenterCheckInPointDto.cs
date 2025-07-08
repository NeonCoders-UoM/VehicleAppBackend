namespace Vpassbackend.DTOs
{
    public class ServiceCenterCheckInPointDto
    {
        public int StationId { get; set; }
        public int Station_id { get; set; }
        public string? Name { get; set; }
    }

    public class ServiceCenterCheckInPointCreateDto
    {
        public int Station_id { get; set; }
        public string? Name { get; set; }
    }

    public class ServiceCenterCheckInPointUpdateDto
    {
        public string? Name { get; set; }
    }
}
