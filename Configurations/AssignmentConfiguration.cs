using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using ToDoApp.Models;

namespace ToDoApp.Configuration
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
    {
        public void Configure(EntityTypeBuilder<TaskAssignment> builder)
        {

            builder.HasKey(ta => new { ta.TaskId, ta.UserId });

            builder.HasOne(ta => ta.Task)
                  .WithMany(t => t.Assignments)
                  .HasForeignKey(ta => ta.TaskId);

            builder.HasOne(ta => ta.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.UserId);
        }
    }
}
