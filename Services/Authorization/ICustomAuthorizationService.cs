using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Services.Authorization
{
    public interface ICustomAuthorizationService
    {
        Task<bool> AuthorizeTaskAccessAsync(CurrentUserContext user, Guid taskId, TaskPermission permission);
        Task<bool> CanUserAssignToUsersAsync(CurrentUserContext user, Guid taskId, List<Guid> assigneeIds);
    }
}
