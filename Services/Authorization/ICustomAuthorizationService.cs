using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Services.Authorization
{
    public interface ICustomAuthorizationService
    {
        Task<bool> AuthorizeTaskAccessAsync(CurrentUserContext user, Guid taskId, TaskPermission permission);
    }
}
