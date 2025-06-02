using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Hospital.DAL.Entities;
using Hospital.DAL;

namespace Hospital.BLL.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllAsync()
        {
            var doctors = await _unitOfWork.Doctors.GetAllAsync();
            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorDto>> GetByGroupAsync(int groupId)
        {
            var doctors = await _unitOfWork.Doctors.GetByGroupAsync(groupId);
            return doctors.Select(MapToDto);
        }

        public async Task<IEnumerable<DoctorDto>> SearchAsync(string? groupName, string? lastName, string? firstName)
        {
            var doctors = await _unitOfWork.Doctors.SearchAsync(groupName, lastName, firstName);
            return doctors.Select(MapToDto);
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            return doctor == null ? null : MapToDto(doctor);
        }

        public async Task<DoctorDto> CreateAsync(DoctorCreateDto doctorDto)
        {
            var group = await _unitOfWork.DoctorGroups.GetByIdAsync(doctorDto.GroupId);
            if (group == null)
                throw new KeyNotFoundException("Doctor group not found");

            var doctor = new Doctor
            {
                FirstName = doctorDto.FirstName,
                LastName = doctorDto.LastName,
                Description = doctorDto.Description,
                GroupId = doctorDto.GroupId
            };
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(doctor);
        }

        public async Task<DoctorDto?> UpdateAsync(int id, DoctorUpdateDto doctorDto)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return null;

            if (doctorDto.FirstName != null)
                doctor.FirstName = doctorDto.FirstName;
            if (doctorDto.LastName != null)
                doctor.LastName = doctorDto.LastName;
            if (doctorDto.Description != null)
                doctor.Description = doctorDto.Description;
            if (doctorDto.GroupId.HasValue)
            {
                var group = await _unitOfWork.DoctorGroups.GetByIdAsync(doctorDto.GroupId.Value);
                if (group == null)
                    throw new KeyNotFoundException("Doctor group not found");
                doctor.GroupId = doctorDto.GroupId.Value;
            }
            _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(doctor);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(id);
            if (doctor == null) return false;
            _unitOfWork.Doctors.Remove(doctor);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static DoctorDto MapToDto(Doctor d) => new DoctorDto
        {
            Id = d.Id,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Description = d.Description,
            GroupId = d.GroupId,
            GroupName = d.Group?.Name
        };
    }
}