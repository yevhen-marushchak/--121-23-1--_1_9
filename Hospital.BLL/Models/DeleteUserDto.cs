namespace Hospital.WebAPI.Models
{
    public class DeleteUserDto
    {
        public int? TargetUserId { get; set; }
        public string? CurrentPassword { get; set; } // лише для пацієнта
    }
}