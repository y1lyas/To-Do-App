using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;
using ToDoApp.Services;


namespace ToDoApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var tasks = await _taskService.GetByUserAsync(userId);
            return Ok(tasks);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();
            var task = await _taskService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id,TaskUpdateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateAsync(userId, id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var deleted = await _taskService.DeleteAsync(userId, id);
            if (!deleted) return NotFound();

            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllAsync();
            return Ok(tasks);
        }
    }
}
