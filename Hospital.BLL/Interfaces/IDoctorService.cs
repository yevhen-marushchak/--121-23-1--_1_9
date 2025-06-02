using Hospital.WebAPI.Models;

namespace Hospital.BLL.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllAsync();
        Task<IEnumerable<DoctorDto>> GetByGroupAsync(int groupId);
        Task<IEnumerable<DoctorDto>> SearchAsync(string? groupName, string? lastName, string? firstName);
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto> CreateAsync(DoctorCreateDto doctorDto);
        Task<DoctorDto?> UpdateAsync(int id, DoctorUpdateDto doctorDto);
        Task<bool> DeleteAsync(int id);
    }
}