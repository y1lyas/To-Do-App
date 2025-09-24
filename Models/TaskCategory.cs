namespace ToDoApp.Models
{
    public class TaskCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    }

}
