using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ToDoApp.Models;

namespace ToDoApp.DTOs.Task
{
    public class TaskCreateDto
    {
        [Required(ErrorMessage = "Title cannot be empty.")]
        [StringLength(100, ErrorMessage = "Title cannot be over 100 characters.")]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public Guid? CategoryId { get; set; }


    }
}
