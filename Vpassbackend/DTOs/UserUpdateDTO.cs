public class UserUpdateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int UserRoleId { get; set; }
    public string? Password { get; set; } // Optional for update
}
