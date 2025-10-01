using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Globalization;
using ToDoApp.DTOs.Task;
using ToDoApp.Extensions;
using ToDoApp.Infrastructure;
using ToDoApp.Mapper;
using ToDoApp.Models;
using ToDoApp.Models.Auth;

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
                CreatedById = userId,
                CategoryId = dto.CategoryId
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            await _context.Entry(task).Reference(t => t.User).LoadAsync();
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            return TaskMapper.MapToDto(task);
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllAsync( Guid? userId = null, Guid? categoryId = null,string ? searchTitle = null,bool? isCompleted = null,string? sortBy = null,bool ascending = true,int page = 1,int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var tasksQuery = _context.Tasks
        .Include(t => t.User)
        .Include(t => t.Category)
        .AsQueryable();

            tasksQuery = tasksQuery.ApplyFilters(requesterRole: "Admin", userId: userId, categoryId, searchTitle, isCompleted, sortBy, ascending, page, pageSize, dueDateFrom, dueDateTo);

            var tasks = await tasksQuery.AsNoTracking().ToListAsync();

            return tasks.Select(TaskMapper.MapToDto);

        }

        public async Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid requesterId, Guid? categoryId = null, string? searchTitle = null, bool? isCompleted = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var tasksQuery = _context.Tasks
              .Where(t => t.UserId == requesterId)
              .Include(t => t.User) 
              .Include(t => t.Category)
              .AsQueryable();

            tasksQuery = tasksQuery.ApplyFilters(requesterRole: "User", userId: null, categoryId, searchTitle, isCompleted, sortBy, ascending, page, pageSize, dueDateFrom, dueDateTo);

            var tasks = await tasksQuery.ToListAsync();
            return tasks.Select(TaskMapper.MapToDto);
        }

        public async Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto)
        {
            var task = await _context.Tasks
              .Include(t => t.User)
              .Include(t => t.Category)
              .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if (task == null) return null;

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.IsCompleted = dto.IsCompleted;
            task.Priority = dto.Priority;
            task.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            // Navigation propertyleri tekrar yükle
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            await _context.Entry(task).Reference(t => t.User).LoadAsync();

            return TaskMapper.MapToDto(task);
        }
        public async Task<int> DeleteCompletedTasksAsync(Guid userId)
        {
            var completedTasks = await _context.Tasks
                .Where(t => t.UserId == userId && t.IsCompleted)
                .ToListAsync();

            if (!completedTasks.Any())
                return 0;

            _context.Tasks.RemoveRange(completedTasks);
            await _context.SaveChangesAsync();

            return completedTasks.Count;
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId)
        {
             var task = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            return TaskMapper.MapToDto(task);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetSubordinatesTasksAsync( Guid captainId ,Guid? userId = null, Guid? categoryId = null,  string? searchTitle = null , bool? isCompleted = null , string? sortBy = null,  bool ascending = true , int page = 1,int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == captainId)
                .Select(u => u.Id)
                .ToListAsync();

            IQueryable<TaskItem> query = _context.Tasks
                .Where(t => subordinateIds.Contains(t.UserId))
                .Include(t => t.User)
                .Include(t => t.Category);

            query = query.ApplyFilters(
                requesterRole: "Captain",
                userId: userId, 
                categoryId: categoryId,
                searchTitle: searchTitle,
                isCompleted: isCompleted,
                sortBy: sortBy,
                ascending: ascending,
                page: page,
                pageSize: pageSize,
                dueDateFrom: dueDateFrom,
                dueDateTo: dueDateTo
            );

            var tasks = await query.ToListAsync();
            return tasks.Select(TaskMapper.MapToDto);
        } 

        public async Task<TaskResponseDto?> CreateTaskForSubordinatesAsync(Guid captainId, TaskCreateDto dto, List<Guid>? subordinateIds = null)
        {
            if (subordinateIds != null && subordinateIds.Any())
            {
                     subordinateIds = await _context.Users
                    .Where(u => subordinateIds.Contains(u.Id) && u.ManagerId == captainId)
                    .Select(u => u.Id)
                    .ToListAsync();
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow,
                CreatedById = captainId,
                CategoryId = dto.CategoryId

            };

            if (subordinateIds != null && subordinateIds.Any())
            {
                foreach (var userId in subordinateIds)
                {
                    task.Assignments.Add(new TaskAssignment
                    {
                        TaskId = task.Id,
                        UserId = userId
                    });
                }
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _context.Entry(task).Collection(t => t.Assignments).LoadAsync();
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            await _context.Entry(task).Reference(t => t.CreatedBy).LoadAsync();

            return TaskMapper.MapToDto(task);
        }

        public async Task<TaskResponseDto?> UpdateSubordinateTaskAsync(Guid captainId, Guid taskId, TaskUpdateDto dto)
        {
            var task = await _context.Tasks
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                t.Assignments.Any(a => a.User.ManagerId == captainId));

            if (task == null) return null;

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.Priority = dto.Priority;
            task.IsCompleted = dto.IsCompleted;
            task.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();
            return TaskMapper.MapToDto(task);
        }

        public async Task<bool> DeleteSubordinateTaskAsync(Guid captainId, Guid taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Assignments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(t => t.Id == taskId &&
                    t.Assignments.Any(a => a.User.ManagerId == captainId));

            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetMyAssignmentsAsync(Guid userId)
        {
            var tasks = await _context.TaskAssignments
                .Where(a => a.UserId == userId)
                .Include(a => a.Task)
                    .ThenInclude(t => t.Category)
                .Include(a => a.Task)
                    .ThenInclude(t => t.CreatedBy)
                .Select(a => a.Task)
                .ToListAsync();

            return tasks.Select(TaskMapper.MapToDto);
        }
    }
}
