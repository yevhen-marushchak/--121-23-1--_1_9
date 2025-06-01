namespace Hospital.DAL.Entities
{
    public class Doctor
    {
        public int Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int GroupId { get; set; }
        public DoctorGroup Group { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}