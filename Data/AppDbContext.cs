using Microsoft.EntityFrameworkCore;
using ToDoApp.Domain;

namespace ToDoApp.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<User> Users { get; set; }

    }
}
