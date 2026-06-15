using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRProject.DTO;
using SignalRProject.Hubs;
using SignalRProject.Interfaces;
using System.Security.Claims;

namespace SignalRProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IChatRoomService _chatRoomService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IImageService _imageService;

        public UserController(IUserService userService, IWebHostEnvironment hostingEnvironment, IChatRoomService chatRoomService, IHubContext<ChatHub> hubContext, IImageService imageService)
        {
            _userService = userService;
            _hostingEnvironment = hostingEnvironment;
            _chatRoomService = chatRoomService;
            _hubContext = hubContext;
            _imageService = imageService;
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0) return BadRequest("Image is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var fileName = await _imageService.SaveAndCompressImageAsync(image, "avatars", 256, 256, true);

            var avatarUrl = "/uploads/avatars/" + fileName;
            var success = await _userService.UpdateUserInfoAsync(userId, new UpdateUserRequestDTO { ProfilePictureUrl = avatarUrl });

            if (!success) return BadRequest("Failed to update user profile picture.");

            var userChats = await _chatRoomService.GetUserChatRoomsAsync(userId);
            foreach (var chat in userChats)
            {
                var members = await _chatRoomService.GetChatRoomMembersAsync(chat.ChatId.ToString());
                var connectionIds = members.Where(m => m.ConnectionId != null).Select(m => m.ConnectionId!).ToList();
                foreach (var connId in connectionIds.Distinct())
                {
                    await _hubContext.Clients.Client(connId).SendAsync("RoomUpdated", chat.ChatId.ToString());
                }
            }

            return Ok(new { ProfilePictureUrl = avatarUrl, Message = "Avatar updated successfully" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _userService.UpdateUserInfoAsync(userId, request);
            if (!success) return BadRequest("Failed to update user info.");

            var userChats = await _chatRoomService.GetUserChatRoomsAsync(userId);
            foreach (var chat in userChats)
            {
                var members = await _chatRoomService.GetChatRoomMembersAsync(chat.ChatId.ToString());
                var connectionIds = members.Where(m => m.ConnectionId != null).Select(m => m.ConnectionId!).ToList();
                foreach (var connId in connectionIds.Distinct())
                {
                    await _hubContext.Clients.Client(connId).SendAsync("RoomUpdated", chat.ChatId.ToString());
                }
            }

            return Ok(new { Message = "User info updated successfully" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var users = await _userService.SearchUsersAsync(query);
            return Ok(users);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var currentUser = await _userService.GetUserByIdAsync(userId);
            if (currentUser == null) return NotFound();

            return Ok(currentUser);
        }
    }
}
