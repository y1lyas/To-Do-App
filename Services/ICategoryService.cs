using ToDoApp.DTOs.Task;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public interface ICategoryService
    {
        public Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        public Task<TaskCategory> CreateCategoryAsync(CreateCategoryDto dto);
    }
}
