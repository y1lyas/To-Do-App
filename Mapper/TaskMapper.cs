using ToDoApp.DTOs.Task;
using ToDoApp.Models;

namespace ToDoApp.Mapper
{
    public static class TaskMapper
    {
        public static TaskResponseDto MapToDto(TaskItem task)
        {
            if (task == null) return null!;

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                Priority = task.Priority,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId,
                Username = task.User?.Username,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name
            };
        }
    }

}
