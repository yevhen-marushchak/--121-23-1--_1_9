namespace Hospital.WebAPI.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}