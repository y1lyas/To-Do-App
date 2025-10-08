namespace ToDoApp.Models.Auth
{
    public class CurrentUserContext
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
