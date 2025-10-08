using System.Text.Json.Serialization;
using ToDoApp.Models;
using TaskStatus = ToDoApp.Models.TaskStatus;

namespace ToDoApp.DTOs.Task
{
    public class TaskResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public TaskStatus Status { get; set; }
        public List<AssignedUserDto> AssignedUsers { get; set; } = new();



    }
}
