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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

        private string GetUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "";

        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var callerRole = GetUserRole();

            if (callerRole == "Administrator")
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            else if (callerRole == "Manager")
            {
                var users = await _userService.GetAllUsersAsync();
                var patients = users.Where(u => u.Role == "Patient");
                return Ok(patients);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var callerRole = GetUserRole();

            if (callerRole == "Patient")
            {
                return Forbid(); 
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (callerRole == "Manager" && user.Role != "Patient")
            {
                return Forbid(); 
            }

            return Ok(user);
        }

        [HttpPost("change-role/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeRole(int id, [FromBody] string newRole)
        {
            var result = await _userService.ChangeRoleAsync(id, newRole);
            if (!result) return BadRequest("Не вдалося змінити роль.");
            return Ok();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var callerId = GetUserId();
            var callerRole = GetUserRole();
            var result = await _userService.ChangePasswordAsync(
                dto.TargetUserId ?? callerId,
                callerId,
                dto.NewPassword,
                dto.CurrentPassword,
                callerRole
            );
            if (!result) return BadRequest("Не вдалося змінити пароль.");
            return Ok();
        }

        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteUserDto dto)
        {
            var callerId = GetUserId();
            var callerRole = GetUserRole();
            var result = await _userService.DeleteAccountAsync(
                dto.TargetUserId ?? callerId,
                callerId,
                callerRole,
                dto.CurrentPassword
            );
            if (!result) return BadRequest("Не вдалося видалити акаунт.");
            return Ok();
        }
    }
}