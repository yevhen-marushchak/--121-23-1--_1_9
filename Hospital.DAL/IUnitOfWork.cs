using Hospital.DAL.Entities;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IDoctorRepository Doctors { get; }
        IGenericRepository<DoctorGroup> DoctorGroups { get; }
        IAppointmentRepository Appointments { get; }
        Task<int> SaveChangesAsync();
    }
}