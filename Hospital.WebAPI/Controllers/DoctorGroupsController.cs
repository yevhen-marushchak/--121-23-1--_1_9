using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorGroupsController : ControllerBase
    {
        private readonly IDoctorGroupService _groupService;

        public DoctorGroupsController(IDoctorGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _groupService.GetAllAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var group = await _groupService.GetByIdAsync(id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([FromBody] DoctorGroupCreateDto dto)
        {
            var group = await _groupService.CreateAsync(new WebAPI.Models.DoctorGroupCreateDto
            {
                Name = dto.Name
            });
            return Ok(group);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorGroupUpdateDto dto)
        {
            var group = await _groupService.UpdateAsync(id, new WebAPI.Models.DoctorGroupUpdateDto
            {
                Name = dto.Name
            });
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _groupService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}