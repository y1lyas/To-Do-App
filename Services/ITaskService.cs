using Microsoft.AspNetCore.Mvc.RazorPages;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid userId, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true,int page = 1, int pageSize = 20);
        Task<TaskResponseDto> CreateAsync(Guid userId, TaskCreateDto dto);
        Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto);
        Task<bool> DeleteAsync(Guid userId, Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20);
        Task<int> DeleteCompletedTasksAsync(Guid userId);
        Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId);

    }
}
