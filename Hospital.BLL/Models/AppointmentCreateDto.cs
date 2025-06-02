namespace Hospital.WebAPI.Models
{
    public class AppointmentCreateDto
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}