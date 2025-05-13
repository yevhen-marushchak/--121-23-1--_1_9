using System.Threading.Tasks;
using Hospital.DAL.Entities;
using Hospital.DAL.Repositories;

namespace Hospital.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HospitalDbContext _context;

        public UnitOfWork(HospitalDbContext context)
        {
            _context = context;

            Doctors = new Repository<Doctor>(context);
            Patients = new Repository<Patient>(context);
            Appointments = new Repository<Appointment>(context);
        }

        public IRepository<Doctor> Doctors { get; private set; }
        public IRepository<Patient> Patients { get; private set; }
        public IRepository<Appointment> Appointments { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}