using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Hospital.DAL.Entities;
using Hospital.DAL;

namespace Hospital.BLL.Services
{
    public class DoctorGroupService : IDoctorGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorGroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorGroupDto>> GetAllAsync()
        {
            var groups = await _unitOfWork.DoctorGroups.GetAllAsync();
            return groups.Select(MapToDto);
        }

        public async Task<DoctorGroupDto?> GetByIdAsync(int id)
        {
            var group = await _unitOfWork.DoctorGroups.GetByIdAsync(id);
            return group == null ? null : MapToDto(group);
        }

        public async Task<DoctorGroupDto> CreateAsync(DoctorGroupCreateDto groupDto)
        {
            var group = new DoctorGroup
            {
                Name = groupDto.Name
            };
            await _unitOfWork.DoctorGroups.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(group);
        }

        public async Task<DoctorGroupDto?> UpdateAsync(int id, DoctorGroupUpdateDto groupDto)
        {
            var group = await _unitOfWork.DoctorGroups.GetByIdAsync(id);
            if (group == null) return null;
            if (groupDto.Name != null)
                group.Name = groupDto.Name;
            _unitOfWork.DoctorGroups.Update(group);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(group);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await _unitOfWork.DoctorGroups.GetByIdAsync(id);
            if (group == null) return false;
            _unitOfWork.DoctorGroups.Remove(group);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static DoctorGroupDto MapToDto(DoctorGroup g) => new DoctorGroupDto
        {
            Id = g.Id,
            Name = g.Name
        };
    }
}