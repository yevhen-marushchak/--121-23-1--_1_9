using Hospital.WebAPI.Models;

namespace Hospital.BLL.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId);
        Task<AppointmentDto?> GetByIdAsync(int id, int userId, string role);
        Task<AppointmentDto> CreateAsync(AppointmentCreateDto appointmentDto, int patientId);
        Task<AppointmentDto?> UpdateAsync(int id, AppointmentUpdateDto appointmentDto, int userId, string role);
        Task<bool> DeleteAsync(int id, int userId, string role);
    }
}