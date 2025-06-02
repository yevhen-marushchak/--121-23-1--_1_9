using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Hospital.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthController(IUserService userService, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var user = await _userService.RegisterAsync(new WebAPI.Models.UserRegisterDto
            {
                Login = dto.Login,
                Password = dto.Password,
                LastName = dto.LastName,
                FirstName = dto.FirstName
            });
            if (user == null)
                return BadRequest("Логін вже зайнято.");

            return Ok(new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                LastName = user.LastName,
                FirstName = user.FirstName,
                Role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var user = await _userService.AuthenticateAsync(dto.Login, dto.Password);
            if (user == null)
                return Unauthorized("Неправильний логін або пароль.");

            var token = _jwtTokenGenerator.GenerateToken(user);
            return Ok(new { Token = token, User = user });
        }
    }
}