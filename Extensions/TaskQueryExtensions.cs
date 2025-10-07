using ToDoApp.Models;

namespace ToDoApp.Extensions
{
    public static class TaskQueryExtensions
    {
        public static IQueryable<TaskItem> ApplyFilters(
       this IQueryable<TaskItem> query, string requesterRole, Guid? userId = null, Guid? categoryId = null, string? searchTitle = null, TaskAssignmentStatus? status = null, string? sortBy = null, bool ascending = true, int page = 1, int pageSize = 20, DateTime? dueDateFrom = null, DateTime? dueDateTo = null)
        {
            if ((requesterRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
             requesterRole.Equals("Captain", StringComparison.OrdinalIgnoreCase))
            && userId.HasValue)
            {
                query = query.Where(t => t.Assignments.Any(a => a.UserId == userId.Value));
            }
            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(t => t.Title.Contains(searchTitle));
            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (status.HasValue)
            {
                query = query.Where(t => t.Assignments.Any(a => a.Status == status.Value));
            }
            if (dueDateFrom.HasValue)
                query = query.Where(t => t.DueDate >= dueDateFrom.Value);

            if (dueDateTo.HasValue)
                query = query.Where(t => t.DueDate <= dueDateTo.Value);

            query = sortBy switch
            {
                "Title" => ascending ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
                "DueDate" => ascending ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
                "Priority" => ascending ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
                _ => query.OrderBy(t => t.CreatedAt)
            };

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query;
        }
    }
}
