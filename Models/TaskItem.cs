using ToDoApp.Models.Auth;

namespace ToDoApp.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public TaskPriority Priority { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? CategoryId { get; set; }
        public TaskCategory? Category { get; set; }

    }
}
