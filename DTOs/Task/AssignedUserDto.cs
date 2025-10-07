namespace ToDoApp.DTOs.Task
{
    public class AssignedUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
