using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToDoApp.Models.Auth
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
        [JsonIgnore]
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public Guid? ManagerId { get; set; }
        [JsonIgnore]
        public User? Manager { get; set; }
        [JsonIgnore]
        public ICollection<User> Subordinates { get; set; } = new List<User>();
        [JsonIgnore]
        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    }
}
