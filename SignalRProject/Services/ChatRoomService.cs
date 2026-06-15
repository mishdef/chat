using Domain;
using Microsoft.AspNetCore.SignalR;
using SignalRProject.Data;
using SignalRProject.Hubs;
using SignalRProject.Interfaces;
using SignalRProject.Model;
using Microsoft.EntityFrameworkCore;

namespace SignalRProject.Services
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatRoomService(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendMessageAsync(Message message)
        {
            await _context.AddAsync(message);
            await _context.SaveChangesAsync();
            
            var chatRoom = await _context.ChatRooms.FindAsync(message.ChatId);
            if (chatRoom != null)
            {
                chatRoom.LastMessageId = message.Id;
                await UpdateChatRoomAsync(chatRoom);
            }

            var members = await GetChatRoomMembersAsync(message.ChatId.ToString());
            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await _hubContext.Clients.Client(member.ConnectionId).SendAsync("RecieveMessage", message);
                }
            }
        }

        public async Task CreateChatRoomAsync(ChatRoom chatRoom)
        {
            await _context.AddAsync(chatRoom);
            await _context.SaveChangesAsync();
            var members = await GetChatRoomMembersAsync(chatRoom.ChatId.ToString());
            foreach (var member in members)
            {
                if (member.ConnectionId != null)
                {
                    await _hubContext.Clients.Client(member.ConnectionId).SendAsync("ChatRoomCreated", chatRoom.ChatId.ToString());
                }
            }
        }

        public async Task DeleteChatRoomAsync(string roomId)
        {
            var chatRoom = await GetChatRoomAsync(roomId);
            if (chatRoom != null)
            {
                var members = chatRoom.Users.ToList();
                _context.Remove(chatRoom);
                await _context.SaveChangesAsync();
                foreach (var member in members)
                {
                    if (member.ConnectionId != null)
                    {
                        await _hubContext.Clients.Client(member.ConnectionId).SendAsync("RoomDeleted", roomId);
                    }
                }
            }
        }

        public async Task<List<Message>> GetChatHistoryAsync(string roomId)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Where(m => m.ChatId == Guid.Parse(roomId))
                .ToListAsync();
        }

        public async Task<ChatRoom?> GetChatRoomAsync(string roomId)
        {
            return await _context.ChatRooms
                .Include(c => c.Users)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.ChatId == Guid.Parse(roomId));
        }

        public async Task<List<User>> GetChatRoomMembersAsync(string roomId)
        {
            var chatRoom = await GetChatRoomAsync(roomId);
            return chatRoom?.Users?.ToList() ?? new List<User>();
        }

        public async Task UpdateChatRoomAsync(ChatRoom chatRoom, List<string>? extraConnectionIdsToNotify = null)
        {
            await _context.SaveChangesAsync();
            var members = await GetChatRoomMembersAsync(chatRoom.ChatId.ToString());
            var connectionIds = members
                .Where(m => m.ConnectionId != null)
                .Select(m => m.ConnectionId!)
                .ToList();

            if (extraConnectionIdsToNotify != null)
            {
                connectionIds.AddRange(extraConnectionIdsToNotify);
            }

            foreach (var connId in connectionIds.Distinct())
            {
                await _hubContext.Clients.Client(connId).SendAsync("RoomUpdated", chatRoom.ChatId.ToString());
            }
        }

        public async Task<bool> IsMemberAsync(string userId, string roomId)
        {
            var chatRoom = await GetChatRoomAsync(roomId);
            if (chatRoom == null)
            {
                return false;
            }

            return chatRoom.Users.Any(x => x.Id == userId);
        }

        public async Task<List<ChatRoom>> GetUserChatRoomsAsync(string userId)
        {
            return await _context.ChatRooms
                .Include(c => c.Users)
                .Include(c => c.LastMessage).ThenInclude(m => m.User)
                .Where(c => c.Users.Any(u => u.Id == userId))
                .ToListAsync();
        }
    }
}
