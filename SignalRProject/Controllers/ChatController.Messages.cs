using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalRProject.DTO;
using SignalRProject.Model;
using System.Security.Claims;

namespace SignalRProject.Controllers
{
    public partial class ChatController
    {
        [Authorize]
        [HttpGet("{id}/history")]
        public async Task<ApiResponse<ChatDTO>> GetChatRoomHistory([FromRoute(Name = "id")] string chatRoomId, [FromQuery] int startIndex, [FromQuery] int endIndex)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<ChatDTO>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatHistoryAsync(chatRoomId);

            var chatDTO = new ChatDTO(
                chatRoom.Skip(startIndex).Take(endIndex - startIndex).ToList(), 
                startIndex, 
                endIndex, 
                chatRoom.Count);

            return ApiResponse<ChatDTO>.Ok(chatDTO, "Chat room found successfully");
        }

        [Authorize]
        [HttpPost("{id}/message")]
        public async Task<ApiResponse<Message>> SendMessage([FromRoute(Name = "id")] string chatRoomId, [FromForm] string messageType, [FromForm] string? messageText = "", [FromForm] IFormFile? image = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<Message>.Forbidden("You are not a member of this chat room");
            }

            if (string.IsNullOrEmpty(messageText) && image == null)
            {
                return ApiResponse<Message>.BadRequest("Message text or image is required");
            }

            var fileName = "";
            if (image != null)
            {
                fileName = await _imageService.SaveAndCompressImageAsync(image, "", 1280, 720, false);
            }

            var message = new Message()
            {
                ChatId = Guid.Parse(chatRoomId),
                Chat = await _chatRoomService.GetChatRoomAsync(chatRoomId),
                UserId = user.Id,
                User = user,
                Content = messageType == "image" ? fileName : (messageText ?? ""),
                Type = messageType,
                CreatedAt = DateTime.UtcNow
            };

            await _chatRoomService.SendMessageAsync(message);

            return ApiResponse<Message>.Ok(message, "Message sent successfully");
        }
    }
}
