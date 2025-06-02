namespace Hospital.WebAPI.Models
{
    public class UserRegisterDto
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
    }
}