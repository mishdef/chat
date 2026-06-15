using Domain;
using SignalRProject.Model;

namespace SignalRProject.Interfaces
{
    public interface IChatRoomService
    {
        Task<ChatRoom> GetChatRoomAsync(string roomId);
        Task CreateChatRoomAsync(ChatRoom chatRoom);
        Task DeleteChatRoomAsync(string roomId);
        Task UpdateChatRoomAsync(ChatRoom chatRoom, List<string>? extraConnectionIdsToNotify = null);
        Task<List<ChatRoom>> GetUserChatRoomsAsync(string userId);


        Task<List<Message>> GetChatHistoryAsync(string roomId);
        Task SendMessageAsync(Message message);
        Task<List<User>> GetChatRoomMembersAsync(string roomId);

        Task<bool> IsMemberAsync(string userId, string roomId);
    }
}
