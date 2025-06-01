using Hospital.DAL.Entities;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HospitalDbContext _context;
        public IUserRepository Users { get; }
        public IDoctorRepository Doctors { get; }
        public IGenericRepository<DoctorGroup> DoctorGroups { get; }
        public IAppointmentRepository Appointments { get; }

        public UnitOfWork(HospitalDbContext context,
                          IUserRepository userRepo,
                          IDoctorRepository doctorRepo,
                          IAppointmentRepository appointRepo)
        {
            _context = context;
            Users = userRepo;
            Doctors = doctorRepo;
            DoctorGroups = new GenericRepository<DoctorGroup>(context);
            Appointments = appointRepo;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}