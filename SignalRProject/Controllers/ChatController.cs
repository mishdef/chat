using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignalRProject.DTO;
using SignalRProject.Interfaces;
using SignalRProject.Model;
using System.Security.Claims;

namespace SignalRProject.Controllers
{
    public record ChatDTO (List<Message> Messages, int StartIndex, int EndIndex, int TotalCount);

    [Route("api/[controller]")]
    [ApiController]
    public partial class ChatController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IChatRoomService _chatRoomService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IImageService _imageService;

        public ChatController(IChatRoomService chatRoomService, UserManager<User> userManager, IWebHostEnvironment hostingEnvironment, IImageService imageService)
        {
            _userManager = userManager;
            _chatRoomService = chatRoomService;
            _hostingEnvironment = hostingEnvironment;
            _imageService = imageService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ApiResponse<ChatRoom>> CreateChatRoom([FromBody] string chatRoomName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var chatRoom = new ChatRoom()
            {
                Name = chatRoomName,
                OwnerId = user.Id,
                Owner = user,
                Users = new List<User>() { user }
            };

            await _chatRoomService.CreateChatRoomAsync(chatRoom);

            return ApiResponse<ChatRoom>.Ok(chatRoom, "Chat room created successfully");
        }

        [Authorize]
        [HttpPost("private")]
        public async Task<ApiResponse<ChatRoom>> CreatePrivateChatRoom([FromBody] CreatePrivateChatDTO request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);
            var targetUser = await _userManager.FindByIdAsync(request.TargetUserId);

            if (targetUser == null)
            {
                return ApiResponse<ChatRoom>.BadRequest("Target user not found");
            }

            if (user.Id == targetUser.Id)
            {
                return ApiResponse<ChatRoom>.BadRequest("Cannot create a private chat with yourself");
            }

            // Check if a private chat between these two users already exists
            var userChats = await _chatRoomService.GetUserChatRoomsAsync(user.Id);
            var existingPrivateChat = userChats.FirstOrDefault(c => 
                c.Type == "Private" && 
                c.Users.Any(u => u.Id == targetUser.Id));

            if (existingPrivateChat != null)
            {
                return ApiResponse<ChatRoom>.Ok(existingPrivateChat, "Existing private chat found");
            }

            var chatRoomName = $"Private_{user.Id}_{targetUser.Id}";
            var chatRoom = new ChatRoom()
            {
                Name = chatRoomName,
                Type = "Private",
                OwnerId = user.Id,
                Owner = user,
                Users = new List<User>() { user, targetUser }
            };

            await _chatRoomService.CreateChatRoomAsync(chatRoom);

            return ApiResponse<ChatRoom>.Ok(chatRoom, "Private chat created successfully");
        }

        [Authorize]
        [HttpGet("my-chats")]
        public async Task<ApiResponse<List<ChatRoom>>> GetMyChatRooms()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var chats = await _chatRoomService.GetUserChatRoomsAsync(user.Id);
            return ApiResponse<List<ChatRoom>>.Ok(chats, "Chat rooms retrieved successfully");
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ApiResponse<ChatRoom>> GetChatRoomInfo([FromRoute(Name = "id")] string chatRoomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<ChatRoom>.Forbidden("You are not a member of this chat room");
            }
            else
            {

                var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);
                return ApiResponse<ChatRoom>.Ok(chatRoom, "Chat room found successfully");
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ApiResponse<bool>> DeleteChatRoom([FromRoute(Name = "id")] string chatRoomId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<bool>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);

            if (chatRoom.Type == "Group" && chatRoom.OwnerId != user.Id)
            {
                return ApiResponse<bool>.Forbidden("Only the chat owner can delete a group chat");
            }

            await _chatRoomService.DeleteChatRoomAsync(chatRoomId);

            return ApiResponse<bool>.Ok(true, "Chat room deleted successfully");
        }

        [Authorize]
        [HttpPost("{id}/picture")]
        public async Task<ApiResponse<string>> UploadChatPicture([FromRoute(Name = "id")] string chatRoomId, [FromForm] IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);
            if (!isMember)
            {
                return ApiResponse<string>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);

            if (chatRoom.Type == "Group" && chatRoom.OwnerId != user.Id)
            {
                return ApiResponse<string>.Forbidden("Only the chat owner can change the group picture");
            }
            if (chatRoom.Type == "Private")
            {
                return ApiResponse<string>.BadRequest("Cannot change picture for a private chat");
            }

            if (image == null || image.Length == 0)
            {
                return ApiResponse<string>.BadRequest("Image is required");
            }

            var fileName = await _imageService.SaveAndCompressImageAsync(image, "", 256, 256, true);
            chatRoom.PictureUrl = fileName;
            await _chatRoomService.UpdateChatRoomAsync(chatRoom);

            return ApiResponse<string>.Ok(fileName, "Chat picture updated successfully");
        }

    }
}
