using ToDoApp.DTOs.Task;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid userId);
        Task<TaskResponseDto> CreateAsync(Guid userId, TaskCreateDto dto);
        Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto);
        Task<bool> DeleteAsync(Guid userId, Guid taskId);
        Task<IEnumerable<TaskResponseDto>> GetAllAsync();
    }
}
