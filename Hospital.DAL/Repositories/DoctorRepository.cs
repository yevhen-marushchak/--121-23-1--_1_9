using Hospital.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.DAL.Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(HospitalDbContext context) : base(context) { }

        public async Task<IEnumerable<Doctor>> GetByGroupAsync(int groupId)
        {
            return await _dbSet.Include(d => d.Group)
                               .Where(d => d.GroupId == groupId)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> SearchAsync(string? groupName, string? lastName, string? firstName)
        {
            var query = _dbSet.Include(d => d.Group).AsQueryable();

            if (!string.IsNullOrWhiteSpace(groupName))
                query = query.Where(d => d.Group.Name.Contains(groupName));
            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(d => d.LastName.Contains(lastName));
            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(d => d.FirstName.Contains(firstName));

            return await query.ToListAsync();
        }
    }
}