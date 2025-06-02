namespace Hospital.WebAPI.Models
{
    public class ChangePasswordDto
    {
        public int? TargetUserId { get; set; }
        public string NewPassword { get; set; } = null!;
        public string? CurrentPassword { get; set; } // лише для пацієнта
    }
}