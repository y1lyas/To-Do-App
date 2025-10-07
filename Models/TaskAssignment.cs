using System.Text.Json.Serialization;
using ToDoApp.Models.Auth;

namespace ToDoApp.Models
{
    public class TaskAssignment
    {
        public Guid TaskId { get; set; }
        
        public TaskItem Task { get; set; } = null!;

        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; } = null!;
        public TaskAssignmentStatus Status { get; set; } = TaskAssignmentStatus.Assigned;
        public DateTime? CompletedAt { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}

