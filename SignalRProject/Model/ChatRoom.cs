using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRProject.Model
{
    public class ChatRoom
    {
        [Key]
        public Guid ChatId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "Group";
        public string? PictureUrl { get; set; }

        public Guid? LastMessageId { get; set; }
        [ForeignKey("LastMessageId")]
        public Message? LastMessage { get; set; }



        [InverseProperty("Chats")]
        public List<User> Users { get; set; } = new List<User>();
        [InverseProperty("Chat")]
        public List<Message> Messages { get; set; } = new List<Message>();


        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; } = null!;

    }
}
