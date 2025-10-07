using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ToDoApp.Models;

namespace ToDoApp.DTOs.Task
{
    public class TaskUpdateDto
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid? CategoryId { get; set; }

    }
}
