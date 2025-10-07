using ToDoApp.Models;

namespace ToDoApp.DTOs.Task
{
    public class AssignedUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public TaskAssignmentStatus Status { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
