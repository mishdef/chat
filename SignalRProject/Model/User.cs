using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SignalRProject.Model
{
    [Index(nameof(NickName), IsUnique = true)]
    public class User : IdentityUser
    {
        [Required]
        public string NickName { get; set; } = null!;
        public string ProfilePictureUrl { get; set; } = "/images/user.png";
        public List<ChatRoom> Chats { get; set; } = new List<ChatRoom>();


        public string? ConnectionId { get; set; }
    }
}
