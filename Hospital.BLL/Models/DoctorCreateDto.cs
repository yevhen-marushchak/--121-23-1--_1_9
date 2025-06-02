namespace Hospital.WebAPI.Models
{
    public class DoctorCreateDto
    {
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int GroupId { get; set; }
    }
}