using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRProject.DTO;
using SignalRProject.Model;
using System.Security.Claims;

namespace SignalRProject.Controllers
{
    public partial class ChatController
    {
        [Authorize]
        [HttpGet("{id}/members")]
        public async Task<ApiResponse<List<User>>> GetChatRoomMembers([FromRoute(Name = "id")] string chatRoomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<List<User>>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);
            return ApiResponse<List<User>>.Ok(chatRoom.Users, "Chat room members found successfully");
        }

        [Authorize]
        [HttpPost("{id}/members")]
        public async Task<ApiResponse<List<User>>> AddChatRoomMembers([FromRoute(Name = "id")] string chatRoomId, [FromBody] List<string> userIds)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<List<User>>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);

            if (chatRoom.Type == "Private")
            {
                return ApiResponse<List<User>>.BadRequest("Cannot modify members in a private chat");
            }

            if (chatRoom.OwnerId != user.Id)
            {
                return ApiResponse<List<User>>.Forbidden("Only the chat owner can add members");
            }

            foreach (var userId in userIds)
            {
                var findUser = await _userManager.FindByIdAsync(userId);
                if (findUser != null)
                {
                    chatRoom.Users.Add(findUser);
                }
            }

            await _chatRoomService.UpdateChatRoomAsync(chatRoom);

            return ApiResponse<List<User>>.Ok(chatRoom.Users, "Chat room members found successfully");
        }

        [Authorize]
        [HttpDelete("{id}/members")]
        public async Task<ApiResponse<List<User>>> RemoveChatRoomMembers([FromRoute(Name = "id")] string chatRoomId, [FromBody] List<string> userIds)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId);

            var isMember = await _chatRoomService.IsMemberAsync(user.Id, chatRoomId);

            if (!isMember)
            {
                return ApiResponse<List<User>>.Forbidden("You are not a member of this chat room");
            }

            var chatRoom = await _chatRoomService.GetChatRoomAsync(chatRoomId);

            if (chatRoom.Type == "Private")
            {
                return ApiResponse<List<User>>.BadRequest("Cannot modify members in a private chat");
            }

            // Allow removal if the user is the owner, OR if the user is only removing themselves (leaving)
            bool isSelfRemoval = userIds.Count == 1 && userIds.First() == user.Id;
            if (chatRoom.OwnerId != user.Id && !isSelfRemoval)
            {
                return ApiResponse<List<User>>.Forbidden("Only the chat owner can remove other members");
            }

            // Prevent owner from removing themselves directly, maybe they should delete chat or transfer ownership
            if (chatRoom.OwnerId == user.Id && userIds.Contains(user.Id))
            {
                return ApiResponse<List<User>>.BadRequest("The owner cannot leave the chat. You must delete the chat instead.");
            }

            var removedConnectionIds = new List<string>();
            foreach (var userId in userIds)
            {
                var findUser = await _userManager.FindByIdAsync(userId);
                if (findUser != null)
                {
                    if (findUser.ConnectionId != null)
                    {
                        removedConnectionIds.Add(findUser.ConnectionId);
                    }
                    chatRoom.Users.Remove(findUser);
                }
            }

            await _chatRoomService.UpdateChatRoomAsync(chatRoom, removedConnectionIds);

            return ApiResponse<List<User>>.Ok(chatRoom.Users, "Chat room members removed successfully");
        }
    }
}
