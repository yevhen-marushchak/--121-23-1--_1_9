using Hospital.DAL.Entities;

namespace Hospital.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByLoginAsync(string login);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<bool> AnyUserWithRoleAsync(string roleName);
        Task<List<User>> GetAllWithRolesAsync();
        Task<User?> GetByIdWithRoleAsync(int id);
    }
}