using Hospital.DAL.Entities;

namespace Hospital.DAL.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<bool> IsSlotTakenAsync(int doctorId, DateTime date, TimeSpan time);
        Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
    }
}