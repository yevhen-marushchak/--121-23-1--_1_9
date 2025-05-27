using System.Collections.Generic;

namespace Hospital.DAL.Entities
{
    public class DoctorGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // "ЛОР", "Терапевт" і т.д.
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}