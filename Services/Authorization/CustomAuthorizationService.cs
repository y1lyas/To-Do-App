using Microsoft.EntityFrameworkCore;
using ToDoApp.Infrastructure;
using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Services.Authorization
{
    public class CustomAuthorizationService : ICustomAuthorizationService
    {
        private readonly AppDbContext _context;

        public CustomAuthorizationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AuthorizeTaskAccessAsync(CurrentUserContext user, Guid taskId, TaskPermission permission)
        {
            if (user == null)
                return false;

            var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return false;

            if (user.Role == "Admin")
                return true;

            return permission switch
            {
                TaskPermission.Update => await CanUpdateTask(user, task),
                TaskPermission.Delete => await CanDeleteTask(user, task),
                TaskPermission.Assign => await CanAssignTask(user, task),
                TaskPermission.UpdateStatus => await CanUpdateStatus(user, task),
                _ => false
            };
        }
        private async Task<bool> CanUpdateTask(CurrentUserContext user, TaskItem task)
        {
            // Task'ı oluşturan güncelleyebilir
            if (task.CreatedById == user.Id)
                return true;

            // Captain astlarının task'larını güncelleyebilir
            if (user.Role == "Captain")
            {
                var assignedUserIds = task.Assignments.Select(a => a.UserId).ToList();

                if (assignedUserIds.Any())
                {
                    var isAssignedToSubordinate = await _context.Users
                        .AnyAsync(u => assignedUserIds.Contains(u.Id) && u.ManagerId == user.Id);

                    return isAssignedToSubordinate;
                }
            }

            return false;
        }
        private Task<bool> CanDeleteTask(CurrentUserContext user, TaskItem task)
        {
            // Sadece oluşturan silebilir
            // Captain bile astlarının task'larını silemez
            return Task.FromResult(task.CreatedById == user.Id);
        }

        private async Task<bool> CanAssignTask(CurrentUserContext user, TaskItem task)
        {
            // Task'ı oluşturan atayabilir
            if (task.CreatedById == user.Id)
                return true;

            // Captain astlarına atayabilir (task zaten astlarına atanmışsa)
            if (user.Role == "Captain")
            {
                var assignedUserIds = task.Assignments.Select(a => a.UserId).ToList();

                if (assignedUserIds.Any())
                {
                    var isAssignedToSubordinate = await _context.Users
                        .AnyAsync(u => assignedUserIds.Contains(u.Id) && u.ManagerId == user.Id);

                    return isAssignedToSubordinate;
                }
            }

            return false;
        }
        private Task<bool> CanUpdateStatus(CurrentUserContext user, TaskItem task)
        {
            // Task'ı oluşturan status'ü güncelleyebilir
            if (task.CreatedById == user.Id)
                return Task.FromResult(true);

            // Task'a atanan kendi assignment status'ünü güncelleyebilir
            var isAssigned = task.Assignments.Any(a => a.UserId == user.Id);
            return Task.FromResult(isAssigned);
        }
        public async Task<bool> CanUserAssignToUsersAsync(CurrentUserContext user, Guid taskId, List<Guid> assigneeIds)
        {
            if (user == null || !assigneeIds.Any())
                return false;

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return false;

            // Admin herkese atayabilir
            if (user.Role == "Admin")
                return true;

            // Task'ı oluşturan herkese atayabilir
            if (task.CreatedById == user.Id)
                return true;

            // Captain sadece astlarına atayabilir
            if (user.Role == "Captain")
            {
                var subordinateIds = await _context.Users
                    .Where(u => u.ManagerId == user.Id)
                    .Select(u => u.Id)
                    .ToListAsync();

                return assigneeIds.All(id => subordinateIds.Contains(id));
            }

            // Normal user sadece kendine atayabilir
            return assigneeIds.Count == 1 && assigneeIds[0] == user.Id;
        }
    }

}

