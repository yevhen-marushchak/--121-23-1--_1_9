using Hospital.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.DAL.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(HospitalDbContext context) : base(context) { }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Login == login);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<bool> AnyUserWithRoleAsync(string roleName)
        {
            return await _dbSet.Include(u => u.Role)
                               .AnyAsync(u => u.Role.Name == roleName);
        }

        public async Task<List<User>> GetAllWithRolesAsync()
        {
            return await _dbSet.Include(u => u.Role).ToListAsync();
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            return await _dbSet.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}