using System.Threading.Tasks;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IDoctorRepository Doctors { get; }
        IDoctorGroupRepository DoctorGroups { get; }
        IAppointmentRepository Appointments { get; }
        Task<int> SaveChangesAsync();
    }
}