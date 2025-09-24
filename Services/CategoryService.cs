using Microsoft.EntityFrameworkCore;
using ToDoApp.DTOs.Task;
using ToDoApp.Infrastructure;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskCategory> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new TaskCategory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.TaskCategories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }
        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
                 return await _context.TaskCategories                               
                .Include(c => c.Tasks)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,          
                    Name = c.Name,
                    Tasks = c.Tasks.Select(t => new TaskDto
                    {
                        Id = t.Id,    
                        Title = t.Title
                    }).ToList()
                }).ToListAsync();



        }
    }
}
