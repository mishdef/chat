using System.ComponentModel.DataAnnotations;

namespace SignalRProject.DTO
{
    public class LoginRequestDTO
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
