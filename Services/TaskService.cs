using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
//using ToDoApp.Domain;
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
                CreatedAt = DateTime.UtcNow,
                Priority = dto.Priority,
                UserId = userId,
                CategoryId = dto.CategoryId
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var createdTask = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            return new TaskResponseDto
            {
                Id = createdTask.Id,
                Title = createdTask.Title,
                Description = createdTask.Description,
                DueDate = createdTask.DueDate,
                IsCompleted = createdTask.IsCompleted,
                CreatedAt = createdTask.CreatedAt,
                Priority = createdTask.Priority,
                UserId = createdTask.UserId,
                Username = createdTask.User.Username,
                CategoryId = createdTask.CategoryId,
                CategoryName = createdTask.Category?.Name
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
                .Include(t => t.Category)
                 .Select(t => new TaskResponseDto
                 {
                     Id = t.Id,
                     Title = t.Title,
                     Description = t.Description,
                     DueDate = t.DueDate,
                     IsCompleted = t.IsCompleted,
                     CreatedAt = DateTime.UtcNow,
                     Priority = t.Priority,
                     UserId = t.UserId,
                     Username = t.User.Username,
                     CategoryId = t.CategoryId,
                     CategoryName = t.Category.Name
                 })
                 .ToListAsync();
        }

        public async Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid userId)
        {
            var tasks = await _context.Tasks
               .Where(t => t.UserId == userId)
               .Include(t => t.Category)
               .Include(t => t.User)
               .ToListAsync();

            return tasks.Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate,
                Priority = t.Priority,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId,
                Username = t.User.Username,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name
            });
        }

        public async Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto)
        {
            var task = await _context.Tasks.Include(t => t.User).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) return null;

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.IsCompleted = dto.IsCompleted;
            task.Priority = dto.Priority;
            task.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            // Navigation güncelledik, yoksa Category.Name null gelir
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            await _context.Entry(task).Reference(t => t.User).LoadAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                UserId = task.UserId,
                Username = task.User.Username,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name
            };
        }
    }
}
