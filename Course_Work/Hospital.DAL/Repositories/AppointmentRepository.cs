using Hospital.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital.DAL.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(HospitalDbContext context) : base(context) { }

        public async Task<bool> IsSlotTakenAsync(int doctorId, DateTime date, TimeSpan time)
        {
            return await _dbSet.AnyAsync(a => a.DoctorId == doctorId && a.Date == date && a.Time == time);
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime? date = null)
        {
            var query = _dbSet.Include(a => a.Patient).Include(a => a.Doctor).Where(a => a.DoctorId == doctorId);
            if (date.HasValue)
                query = query.Where(a => a.Date == date.Value);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
        {
            return await _dbSet.Include(a => a.Doctor).ThenInclude(d => d.Group)
                               .Where(a => a.PatientId == patientId)
                               .ToListAsync();
        }
    }
}