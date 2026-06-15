using System.ComponentModel.DataAnnotations;

namespace SignalRProject.DTO
{
    public class RegistrationRequestDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public required string Nickname { get; set; }

        [Required]
        public required string Password { get; set; }
         
    }
}
