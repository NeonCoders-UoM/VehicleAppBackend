using System.Text.Json.Serialization;

namespace Vpassbackend.DTOs
{
    public class UserRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int UserRoleId { get; set; } // 1=SuperAdmin, etc.
        [JsonPropertyName("station_id")]
        public int? Station_id { get; set; } // Required for ServiceCenterAdmin (UserRoleId = 3)
    }
}
