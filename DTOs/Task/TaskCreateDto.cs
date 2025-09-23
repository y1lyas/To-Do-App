using System.ComponentModel.DataAnnotations;

namespace ToDoApp.DTOs.Task
{
    public class TaskCreateDto
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;

    }
}
