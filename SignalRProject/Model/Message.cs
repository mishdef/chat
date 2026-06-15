using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRProject.Model
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }


        public string Type { get; set; }
        public string Content { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public Guid ChatId { get; set; }
        [ForeignKey("ChatId")]
        public ChatRoom Chat { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public List<User> UsersView { get; set; } = new List<User>();
    }
}
