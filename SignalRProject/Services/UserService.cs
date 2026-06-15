using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignalRProject.DTO;
using SignalRProject.Interfaces;
using SignalRProject.Model;

namespace SignalRProject.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequestDTO updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(updateDto.NickName))
                user.NickName = updateDto.NickName;
            
            if (!string.IsNullOrEmpty(updateDto.ProfilePictureUrl))
                user.ProfilePictureUrl = updateDto.ProfilePictureUrl;

            if (!string.IsNullOrEmpty(updateDto.UserName) && updateDto.UserName != user.UserName)
                await _userManager.SetUserNameAsync(user, updateDto.UserName);

            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
                await _userManager.SetEmailAsync(user, updateDto.Email);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<UserResponseDTO>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<UserResponseDTO>();

            query = query.ToLower();
            var users = await _userManager.Users
                .Where(u => (u.UserName != null && u.UserName.ToLower().Contains(query)) || 
                            (u.Email != null && u.Email.ToLower().Contains(query)) ||
                            (u.NickName != null && u.NickName.ToLower().Contains(query)))
                .Take(20)
                .ToListAsync();

            return users.Select(u => new UserResponseDTO
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                NickName = u.NickName,
                ProfilePictureUrl = u.ProfilePictureUrl
            }).ToList();
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                NickName = user.NickName,
                ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
    }
}
