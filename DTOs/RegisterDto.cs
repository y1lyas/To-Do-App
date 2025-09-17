using System.ComponentModel.DataAnnotations;

namespace ToDoApp.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(20)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
