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
            var task = await _context.Tasks
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return false;

            if (task.CreatedById == user.Id)
                return true;

            var userAssignment = task.Assignments.FirstOrDefault(a => a.UserId == user.Id);

            if (userAssignment != null)
            {
                if (permission == TaskPermission.View || permission == TaskPermission.Update)
                    return true;
            }

            if (user.Role == "Captain")
            {
                var assignedUserIds = task.Assignments.Select(a => a.UserId).ToList();

                if (assignedUserIds.Any())
                {
                    var isAssignedToSubordinate = await _context.Users
                        .AnyAsync(u => assignedUserIds.Contains(u.Id) && u.ManagerId == user.Id);

                    if (isAssignedToSubordinate && permission == TaskPermission.Assign)
                        return true;
                }
            }
            return false;
        }

    }
}
