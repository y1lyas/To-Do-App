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
using TaskStatus = ToDoApp.Models.TaskStatus;

namespace ToDoApp.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        private void SyncTaskStatus(TaskItem task)
        {
            if (task.Assignments.All(a => a.Status == TaskAssignmentStatus.Completed))
                task.Status = TaskStatus.Completed;
            else if (task.Assignments.Any(a => a.Status == TaskAssignmentStatus.InProgress))
                task.Status = TaskStatus.Active;
            else
                task.Status = TaskStatus.Pending;
        }
        public async Task<TaskResponseDto> CreateAsync(Guid userId, TaskCreateDto dto)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                Priority = dto.Priority,
                CreatedById = userId,
                CategoryId = dto.CategoryId
            };

            task.Assignments.Add(new TaskAssignment
            {
                TaskId = task.Id,
                UserId = userId,
                AssignedAt = DateTime.UtcNow
            });

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _context.Entry(task).Collection(t => t.Assignments).LoadAsync();
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            await _context.Entry(task).Reference(t => t.CreatedBy).LoadAsync();

            return TaskMapper.MapToDto(task);
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid taskId)
        {
            var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedById == userId);

            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllAsync( Guid? userId = null, Guid? categoryId = null,string ? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null,bool ascending = true,int page = 1,int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var tasksQuery = _context.Tasks
            .Include(t => t.Assignments)
            .ThenInclude(a => a.User)
            .Include(t => t.Category)
            .Include(t => t.CreatedBy)
            .AsQueryable();

            tasksQuery = tasksQuery.ApplyFilters(requesterRole: "Admin", userId: userId, categoryId, searchTitle, status, sortBy, ascending, page, pageSize, dueDateFrom, dueDateTo);

            var tasks = await tasksQuery.AsNoTracking().ToListAsync();

            return tasks.Select(TaskMapper.MapToDto);

        }

        public async Task<IEnumerable<TaskResponseDto>> GetByUserAsync(Guid requesterId, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var query = _context.Tasks
              .Where(t => t.Assignments.Any(a => a.UserId == requesterId) && t.Status != TaskStatus.Archived && t.Status != TaskStatus.Deleted )
               .Include(a => a.Assignments)
                  .ThenInclude(t => t.User)
               .Include(t => t.Category)
               .Include(t => t.CreatedBy)
               .AsQueryable();

            query = query.ApplyFilters(requesterRole: "User", userId: null, categoryId, searchTitle, status, sortBy, ascending, page, pageSize, dueDateFrom, dueDateTo);

            var tasks = await query.ToListAsync();
            return tasks.Select(TaskMapper.MapToDto);
        }

        public async Task<TaskResponseDto?> UpdateAsync(Guid userId, Guid taskId, TaskUpdateDto dto)
        {
            var task = await _context.Tasks
              .Include(t => t.Assignments)
              .Include(t => t.Category)
              .FirstOrDefaultAsync(t => t.Id == taskId && t.Assignments.Any(a => a.UserId == userId));

            if (task == null) return null;

            task.Title = dto.Title ?? task.Title;
            task.Description = dto.Description ?? task.Description;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.Priority = dto.Priority;
            task.CategoryId = dto.CategoryId;

            await _context.SaveChangesAsync();

            await _context.Entry(task).Collection(t => t.Assignments).LoadAsync();
            await _context.Entry(task).Reference(t => t.Category).LoadAsync();
            await _context.Entry(task).Reference(t => t.CreatedBy).LoadAsync();

            return TaskMapper.MapToDto(task);
        }
        public async Task<int> ArchiveCompletedTasksAsync(Guid userId)
        {
            var completedTasks = await _context.Tasks
                .Include(t => t.Assignments)
                .Where(t=>t.Status == TaskStatus.Completed && (t.CreatedById == userId || t.Assignments.Any(a => a.UserId == userId)))
                .ToListAsync();

            if (!completedTasks.Any())
                return 0;

            foreach (var task in completedTasks)
                task.Status = TaskStatus.Archived;

            await _context.SaveChangesAsync();
            return completedTasks.Count;
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid taskId)
        {
             var task = await _context.Tasks
                .Include(t => t.Assignments)
                .ThenInclude(a => a.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            return TaskMapper.MapToDto(task);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetSubordinatesTasksAsync( Guid captainId ,Guid? userId = null, Guid? categoryId = null,  string? searchTitle = null , TaskAssignmentStatus? status = null, string? sortBy = null,  bool ascending = true , int page = 1,int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == captainId)
                .Select(u => u.Id)
                .ToListAsync();

            IQueryable<TaskItem> query = _context.Tasks
                .Where(t => t.Assignments.Any( a => subordinateIds.Contains(a.UserId)))
                .Include(t => t.Assignments)
                .ThenInclude(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.CreatedBy);

            query = query.ApplyFilters(
                requesterRole: "Captain",
                userId: userId, 
                categoryId: categoryId,
                searchTitle: searchTitle,
                status: status,
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
                        UserId = userId,
                        Status = TaskAssignmentStatus.Assigned
                    });
                }
            }

            SyncTaskStatus(task); 

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _context.Entry(task)
                .Collection(t => t.Assignments)
                .Query()
                .Include(a => a.User)
                .LoadAsync();
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
        public async Task<TaskResponseDto?> AssignUserToTaskAsync(Guid taskId, Guid userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Assignments)
                .ThenInclude(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            if (task.Assignments.Any(a => a.UserId == userId)) return TaskMapper.MapToDto(task);

            task.Assignments.Add(new TaskAssignment
            {
                TaskId = task.Id,
                UserId = userId,
                Status = TaskAssignmentStatus.Assigned
            });

            SyncTaskStatus(task);
            await _context.SaveChangesAsync();
            return TaskMapper.MapToDto(task);
        }
        public async Task<TaskResponseDto?> RemoveUserFromTaskAsync(Guid taskId, Guid userId)
        {
            var assignment = await _context.TaskAssignments
             .Include(a => a.Task)
             .ThenInclude(t => t.Assignments)
             .ThenInclude(t => t.User)
             .FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);

            if (assignment == null) return null;

            _context.TaskAssignments.Remove(assignment);

            SyncTaskStatus(assignment.Task);

            await _context.SaveChangesAsync();

            return TaskMapper.MapToDto(assignment.Task);
        }

        public async Task<TaskResponseDto?> UpdateAssignmentStatusAsync(Guid userId, Guid taskId, TaskAssignmentStatus newStatus)
        {
            var assignment = await _context.TaskAssignments
                .Include(a => a.Task)
                .ThenInclude(t => t.Assignments)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);

            if (assignment == null)
                return null;

            assignment.Status = newStatus;
            assignment.CompletedAt = newStatus == TaskAssignmentStatus.Completed ? DateTime.UtcNow : null;

            SyncTaskStatus(assignment.Task);

            await _context.SaveChangesAsync();
            return TaskMapper.MapToDto(assignment.Task);    
        }


    }
}
