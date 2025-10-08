using ToDoApp.Infrastructure;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;
using ToDoApp.Services.Auth;

namespace ToDoApp.Services.Authorization
{
    public class TaskAuthService : ITaskAuthService
    {
        private readonly AppDbContext _context;

        public TaskAuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanModifyTaskAsync(Guid taskId, Guid currentUserId)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return false;

            return task.CreatedById == currentUserId;
        }
    }
}
