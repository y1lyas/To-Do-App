using Microsoft.EntityFrameworkCore;
using ToDoApp.Configuration;
using ToDoApp.Extensions;
using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            modelBuilder.ApplyConfiguration(new AssignmentConfiguration());

            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

            modelBuilder.ApplyConfiguration(new TaskItemConfiguration());

        }

    }
}
