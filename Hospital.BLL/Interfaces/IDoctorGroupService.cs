using Hospital.WebAPI.Models;

namespace Hospital.BLL.Interfaces
{
    public interface IDoctorGroupService
    {
        Task<IEnumerable<DoctorGroupDto>> GetAllAsync();
        Task<DoctorGroupDto?> GetByIdAsync(int id);
        Task<DoctorGroupDto> CreateAsync(DoctorGroupCreateDto groupDto);
        Task<DoctorGroupDto?> UpdateAsync(int id, DoctorGroupUpdateDto groupDto);
        Task<bool> DeleteAsync(int id);
    }
}