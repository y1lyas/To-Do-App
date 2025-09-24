using Microsoft.EntityFrameworkCore;
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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<TaskItem>()
                  .HasOne(t => t.User)
                  .WithMany(u => u.Tasks)
                  .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<TaskItem>()
                  .Property(t => t.CreatedAt)
                  .HasDefaultValueSql("NOW()");
            modelBuilder.Entity<TaskItem>()
                  .Property(t => t.Priority)
                  .HasDefaultValue(TaskPriority.Medium);

            modelBuilder.Entity<TaskItem>()
                  .HasOne(t => t.Category)
                  .WithMany(c => c.Tasks)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

        }

    }
}
