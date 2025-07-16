namespace Vpassbackend.DTOs
{
    public class MarkNotificationAsReadDTO
    {
        public DateTime? ReadAt { get; set; } = DateTime.UtcNow;
    }
}
