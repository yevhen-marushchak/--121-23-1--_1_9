using Hospital.DAL.Entities;

namespace Hospital.DAL.Repositories
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetByGroupAsync(int groupId);
        Task<IEnumerable<Doctor>> SearchAsync(string? groupName, string? lastName, string? firstName);
    }
}