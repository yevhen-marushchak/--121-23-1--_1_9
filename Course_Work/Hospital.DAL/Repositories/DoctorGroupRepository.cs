using Hospital.DAL.Entities;

namespace Hospital.DAL.Repositories
{
    public class DoctorGroupRepository : GenericRepository<DoctorGroup>, IDoctorGroupRepository
    {
        public DoctorGroupRepository(HospitalDbContext context) : base(context) { }
    }
}