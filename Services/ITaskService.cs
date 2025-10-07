using Microsoft.AspNetCore.Mvc.RazorPages;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid requesterId, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<TaskResponseDto> CreateAsync(Guid userId, TaskCreateDto dto);
        Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto);
        Task<bool> DeleteAsync(Guid userId, Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<int> ArchiveCompletedTasksAsync(Guid userId);
        Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetSubordinatesTasksAsync(Guid captainId, Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<TaskResponseDto?> CreateTaskForSubordinatesAsync(Guid captainId, TaskCreateDto dto, List<Guid>? subordinateIds = null);
        Task<TaskResponseDto?> UpdateSubordinateTaskAsync(Guid captainId, Guid taskId, TaskUpdateDto dto);
        Task<bool> DeleteSubordinateTaskAsync(Guid captainId, Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetMyAssignmentsAsync(Guid userId);
        Task<TaskResponseDto?> RemoveUserFromTaskAsync(Guid taskId, Guid userId);
        Task<TaskResponseDto?> AssignUserToTaskAsync(Guid taskId, Guid userId);

    }
}
