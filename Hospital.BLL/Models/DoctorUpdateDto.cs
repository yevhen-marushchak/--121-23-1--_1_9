namespace Hospital.WebAPI.Models
{
    public class DoctorUpdateDto
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Description { get; set; }
        public int? GroupId { get; set; }
    }
}