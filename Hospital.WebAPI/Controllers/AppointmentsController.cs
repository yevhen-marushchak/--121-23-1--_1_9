using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        private string GetUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";

        // --- Лише для адміністратора і менеджера ---
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] DateTime? date = null)
        {
            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId, date);
            return Ok(appointments);
        }

        // --- Лише для пацієнта: користувач бачить лише свої записи ---
        [HttpGet("patient")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetByPatient()
        {
            var userId = GetUserId();
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(userId);
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var appointment = await _appointmentService.GetByIdAsync(id, userId, role);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            var userId = GetUserId();
            var appointment = await _appointmentService.CreateAsync(new AppointmentCreateDto
            {
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                Time = dto.Time
            }, userId);
            return Ok(appointment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Patient,Administrator,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentUpdateDto dto)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var appointment = await _appointmentService.UpdateAsync(id, new AppointmentUpdateDto
            {
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                Time = dto.Time
            }, userId, role);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Patient,Administrator,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var result = await _appointmentService.DeleteAsync(id, userId, role);
            if (!result) return NotFound();
            return Ok();
        }
    }
}