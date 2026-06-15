using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignalRProject.Interfaces;
using SignalRProject.Model;

namespace SignalRProject.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly UserManager<User> _userManager;

        public ChatHub(IChatRoomService chatRoomService, UserManager<User> userManager)
        {
            _userManager = userManager;
            _chatRoomService = chatRoomService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {    
                return;
            }

            user.ConnectionId = Context.ConnectionId;

            await _userManager.UpdateAsync(user);
        }

        public async Task SendMessageToChatRoom(Message message)
        {
            var room = message.ChatId;

            var members = await _chatRoomService.GetChatRoomMembersAsync(room.ToString());

            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await Clients.Client(member.ConnectionId).SendAsync("RecieveMessage", message);
                }
            }
        }

        public async Task ChatRoomCreated(string roomId)
        {
            var members = await _chatRoomService.GetChatRoomMembersAsync(roomId);

            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await Clients.Client(member.ConnectionId).SendAsync("ChatRoomCreated", roomId);
                }
            }
        }

        public async Task ChatRoomDeleted(string roomId)
        {
            var members = await _chatRoomService.GetChatRoomMembersAsync(roomId);

            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await Clients.Client(member.ConnectionId).SendAsync("RoomDeleted", roomId);
                }
            }
        }

        public async Task ChatRoomUpdated(string roomId)
        {
            var members = await _chatRoomService.GetChatRoomMembersAsync(roomId);

            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await Clients.Client(member.ConnectionId).SendAsync("RoomUpdated", roomId);
                }
            }
        }
    }
}
