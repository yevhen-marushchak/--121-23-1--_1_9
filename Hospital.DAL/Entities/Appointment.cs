namespace Hospital.DAL.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public User Patient { get; set; } = null!;
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public DateTime Date { get; set; } 
        public TimeSpan Time { get; set; } 
    }
}