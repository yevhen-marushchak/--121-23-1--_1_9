using System.Threading.Tasks;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HospitalDbContext _context;
        public IUserRepository Users { get; }
        public IDoctorRepository Doctors { get; }
        public IDoctorGroupRepository DoctorGroups { get; }
        public IAppointmentRepository Appointments { get; }

        public UnitOfWork(HospitalDbContext context,
                          IUserRepository userRepo,
                          IDoctorRepository doctorRepo,
                          IDoctorGroupRepository groupRepo,
                          IAppointmentRepository appointRepo)
        {
            _context = context;
            Users = userRepo;
            Doctors = doctorRepo;
            DoctorGroups = groupRepo;
            Appointments = appointRepo;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}