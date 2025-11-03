// DTOs/ChangePasswordDto.cs
namespace Vpassbackend.DTOs
{
    public class ChangePasswordDto
    {
        public int CustomerId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}