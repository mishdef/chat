using SignalRProject.DTO;

namespace SignalRProject.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequestDTO updateDto);
        Task<List<UserResponseDTO>> SearchUsersAsync(string query);
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);
    }
}
