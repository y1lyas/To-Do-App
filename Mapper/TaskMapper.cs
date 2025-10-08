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
                CreatedById = task.CreatedById,
                CreatedByName = task.CreatedBy.Username,
                Status = task.Status,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name,
                 AssignedUsers = task.Assignments?.Select(a => new AssignedUserDto
                 {
                     UserId = a.UserId,
                     Username = a.User?.Username ?? "Unknown",
                     Status = a.Status,
                     AssignedAt = a.AssignedAt
                 }).ToList() ?? new List<AssignedUserDto>(),
            };
        }
    }

}
