using Hospital.WebAPI.Models;

namespace Hospital.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> AuthenticateAsync(string login, string password);
        Task<UserDto?> RegisterAsync(UserRegisterDto registerDto);

        Task<bool> ChangePasswordAsync(int targetUserId, int callerUserId, string newPassword, string? currentPassword, string callerRole);
        Task<bool> DeleteAccountAsync(int targetUserId, int callerUserId, string callerRole, string? currentPassword);

        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> ChangeRoleAsync(int userId, string newRole);
    }
}