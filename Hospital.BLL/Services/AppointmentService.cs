using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Hospital.DAL.Entities;
using Hospital.DAL;

namespace Hospital.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date = null)
        {
            var appointments = await _unitOfWork.Appointments.GetByDoctorAsync(doctorId, date);
            return appointments.Select(MapToDto);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(int patientId)
        {
            var appointments = await _unitOfWork.Appointments.GetByPatientAsync(patientId);
            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id, int userId, string role)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return null;

            // Лише пацієнт (власник), лікар або адміністратор/менеджер можуть переглядати запис
            if (role == "Patient" && appointment.PatientId != userId)
                return null;

            return MapToDto(appointment);
        }

        public async Task<AppointmentDto> CreateAsync(AppointmentCreateDto dto, int patientId)
        {
            if (dto.Date.Date < DateTime.Today)
                throw new InvalidAppointmentTimeException("Cannot book appointments in the past.");

            if (!IsValidTime(dto.Time))
                throw new InvalidAppointmentTimeException("Invalid appointment time. Only :00 or :30 from 8:00 to 18:30 allowed.");

            var doctor = await _unitOfWork.Doctors.GetByIdAsync(dto.DoctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found.");

            bool slotTaken = await _unitOfWork.Appointments.IsSlotTakenAsync(dto.DoctorId, dto.Date.Date, dto.Time);
            if (slotTaken)
                throw new SlotTakenException("This time slot is already taken.");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = patientId,
                Date = dto.Date.Date,
                Time = dto.Time
            };

            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(appointment);
        }

        public async Task<AppointmentDto?> UpdateAsync(int id, AppointmentUpdateDto dto, int userId, string role)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return null;

            // Лише пацієнт (власник), адміністратор, менеджер можуть редагувати
            if (role == "Patient" && appointment.PatientId != userId)
                return null;

            int newDoctorId = dto.DoctorId ?? appointment.DoctorId;
            DateTime newDate = dto.Date?.Date ?? appointment.Date;
            TimeSpan newTime = dto.Time ?? appointment.Time;

            if (newDate < DateTime.Today)
                throw new InvalidAppointmentTimeException("Cannot move appointment to date in the past.");

            if (dto.Time.HasValue && !IsValidTime(newTime))
                throw new InvalidAppointmentTimeException("Invalid appointment time. Only :00 or :30 from 8:00 to 18:30 allowed.");

            if (dto.DoctorId.HasValue && newDoctorId != appointment.DoctorId)
            {
                var doctor = await _unitOfWork.Doctors.GetByIdAsync(newDoctorId);
                if (doctor == null)
                    throw new KeyNotFoundException("Doctor not found.");
            }

            bool slotTaken = await _unitOfWork.Appointments.IsSlotTakenAsync(newDoctorId, newDate, newTime);
            if (slotTaken && (newDoctorId != appointment.DoctorId || newDate != appointment.Date || newTime != appointment.Time))
                throw new SlotTakenException("This time slot is already taken.");

            appointment.DoctorId = newDoctorId;
            appointment.Date = newDate;
            appointment.Time = newTime;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(appointment);
        }

        public async Task<bool> DeleteAsync(int id, int userId, string role)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(id);
            if (appointment == null)
                return false;

            // Лише пацієнт (власник), адміністратор, менеджер можуть видаляти
            if (role == "Patient" && appointment.PatientId != userId)
                return false;

            _unitOfWork.Appointments.Remove(appointment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // Допоміжні методи
        private static bool IsValidTime(TimeSpan time)
        {
            bool halfHour = AppointmentSettings.AllowedMinutes.Contains(time.Minutes);
            bool inRange = time >= AppointmentSettings.StartTime && time <= AppointmentSettings.EndTime;
            return halfHour && inRange;
        }

        private static AppointmentDto MapToDto(Appointment a) => new AppointmentDto
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor != null ? $"{a.Doctor.LastName} {a.Doctor.FirstName}" : null,
            PatientId = a.PatientId,
            PatientName = a.Patient != null ? $"{a.Patient.LastName} {a.Patient.FirstName}" : null,
            Date = a.Date,
            Time = a.Time
        };
    }
}
