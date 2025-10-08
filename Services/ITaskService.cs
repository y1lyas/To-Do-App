using Microsoft.AspNetCore.Mvc.RazorPages;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid requesterId, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<TaskResponseDto> CreateAsync(CurrentUserContext currentUser, TaskCreateDto dto);
        Task<TaskResponseDto?> UpdateAsync(CurrentUserContext currentUser, Guid taskId, TaskUpdateDto dto);
        Task<bool> DeleteAsync(CurrentUserContext currentUser, Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetAllAsync(Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<int> ArchiveCompletedTasksAsync(CurrentUserContext currentUser);
        Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetSubordinatesTasksAsync(Guid captainId, Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);
        Task<TaskResponseDto?> CreateTaskForSubordinatesAsync(Guid captainId, TaskCreateDto dto, List<Guid>? subordinateIds = null);
        Task<TaskResponseDto?> UpdateSubordinateTaskAsync(Guid captainId, Guid taskId, TaskUpdateDto dto);
        Task<TaskResponseDto?> RemoveUserFromTaskAsync(Guid taskId, Guid userId);
        Task<TaskResponseDto?> AssignUserToTaskAsync( Guid taskId, Guid userId);
        Task<TaskResponseDto?> UpdateAssignmentStatusAsync(CurrentUserContext currentUser, Guid taskId ,TaskAssignmentStatus newStatus);
        Task<IEnumerable<TaskResponseDto>> GetMyAssignmentsAsync(CurrentUserContext currentUser, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null);


    }
}
