using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain;
using ToDoApp.DTOs.Task;
using ToDoApp.Infrastructure;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskResponseDto> CreateAsync(Guid userId, TaskCreateDto dto)
        {

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),    
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                IsCompleted = false,
                UserId = userId, 
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId
            };
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllAsync()
        {
            return await _context.Tasks
                 .Select(t => new TaskResponseDto
                 {
                     Id = t.Id,
                     Title = t.Title,
                     Description = t.Description,
                     DueDate = t.DueDate,
                     IsCompleted = t.IsCompleted,
                     UserId = t.UserId,
                     Username = t.User.Username
                 })
                 .ToListAsync();
        }

        public async Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    UserId = t.UserId,
                    Username = t.User.Username
                })
                .ToListAsync();
        }

        public async Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto)
        {
            var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) return null;

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.IsCompleted = dto.IsCompleted;

            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId,
                Username = task.User.Username
            };
        }
    }
}
