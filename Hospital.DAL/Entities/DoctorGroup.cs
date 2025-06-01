namespace Hospital.DAL.Entities
{
    public class DoctorGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "Лор", "Терапевт" тощо
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}