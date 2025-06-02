using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? group, [FromQuery] string? lastName, [FromQuery] string? firstName)
        {
            var doctors = await _doctorService.SearchAsync(group, lastName, firstName);
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([FromBody] DoctorCreateDto dto)
        {
            var doctor = await _doctorService.CreateAsync(new WebAPI.Models.DoctorCreateDto
            {
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Description = dto.Description,
                GroupId = dto.GroupId
            });
            return Ok(doctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorUpdateDto dto)
        {
            var doctor = await _doctorService.UpdateAsync(id, new WebAPI.Models.DoctorUpdateDto
            {
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                Description = dto.Description,
                GroupId = dto.GroupId
            });
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}