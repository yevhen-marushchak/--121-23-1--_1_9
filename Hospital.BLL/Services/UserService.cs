using System.Security.Cryptography;
using System.Text;
using Hospital.BLL.Interfaces;
using Hospital.WebAPI.Models;
using Hospital.DAL.Entities;
using Hospital.DAL;

namespace Hospital.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto?> AuthenticateAsync(string login, string password)
        {
            var user = await _unitOfWork.Users.GetByLoginAsync(login);
            if (user == null) return null;
            if (!VerifyPassword(password, user.PasswordHash)) return null;
            return MapToUserDto(user);
        }

        public async Task<UserDto?> RegisterAsync(UserRegisterDto registerDto)
        {
            var existing = await _unitOfWork.Users.GetByLoginAsync(registerDto.Login);
            if (existing != null) return null;

            bool hasAdmin = await _unitOfWork.Users.AnyUserWithRoleAsync("Administrator");
            string roleName = hasAdmin ? "Patient" : "Administrator";
            var role = await _unitOfWork.Users.GetRoleByNameAsync(roleName);
            if (role == null)
                throw new Exception($"Role '{roleName}' not found.");

            var user = new User
            {
                Login = registerDto.Login,
                PasswordHash = HashPassword(registerDto.Password),
                LastName = registerDto.LastName,
                FirstName = registerDto.FirstName,
                RoleId = role.Id
            };
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            var createdUser = await _unitOfWork.Users.GetByLoginAsync(user.Login);
            return MapToUserDto(createdUser!);
        }

        public async Task<bool> ChangePasswordAsync(int targetUserId, int callerUserId, string newPassword, string? currentPassword, string callerRole)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(targetUserId);
            if (user == null) return false;

            if (callerRole == "Administrator")
            {
                user.PasswordHash = HashPassword(newPassword);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            if (callerRole == "Manager")
            {
                if (user.Role?.Name == "Patient")
                {
                    user.PasswordHash = HashPassword(newPassword);
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
                else return false;
            }
            if (callerRole == "Patient")
            {
                if (targetUserId != callerUserId)
                    return false;
                if (string.IsNullOrEmpty(currentPassword) || !VerifyPassword(currentPassword, user.PasswordHash))
                    return false;
                user.PasswordHash = HashPassword(newPassword);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAccountAsync(int targetUserId, int callerUserId, string callerRole, string? currentPassword)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(targetUserId);
            if (user == null) return false;

            if (callerRole == "Administrator")
            {
                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            if (callerRole == "Manager")
            {
                if (user.Role?.Name == "Patient")
                {
                    _unitOfWork.Users.Remove(user);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                }
                else return false;
            }
            if (callerRole == "Patient")
            {
                if (targetUserId != callerUserId)
                    return false;
                if (string.IsNullOrEmpty(currentPassword) || !VerifyPassword(currentPassword, user.PasswordHash))
                    return false;
                _unitOfWork.Users.Remove(user);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllWithRolesAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdWithRoleAsync(id);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<bool> ChangeRoleAsync(int userId, string newRole)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;
            var role = await _unitOfWork.Users.GetRoleByNameAsync(newRole);
            if (role == null) return false;
            user.RoleId = role.Id;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        private static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;

        private static UserDto MapToUserDto(User user) =>
            new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                LastName = user.LastName,
                FirstName = user.FirstName,
                Role = user.Role?.Name ?? "Unknown"
            };
    }
}