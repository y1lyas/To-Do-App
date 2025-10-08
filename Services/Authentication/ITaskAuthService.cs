namespace ToDoApp.Services.Auth
{
    public interface ITaskAuthService
    {
        Task<bool> CanModifyTaskAsync(Guid taskId, Guid currentUserId);
    }
}
