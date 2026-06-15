using System.ComponentModel.DataAnnotations;

namespace SignalRProject.DTO
{
    public class ClientDTO
    {
        public string Username { get; set; } = default!;
        public string ProfilePictureUrl { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
