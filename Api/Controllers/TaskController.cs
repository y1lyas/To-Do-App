using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
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
        [Authorize]
        [HttpGet("by-user")]
        public async Task<IActionResult> GetTask(
            [FromQuery] string? searchTitle,
            [FromQuery] Guid? categoryId,
            [FromQuery] bool? isCompleted,
            [FromQuery] string? sortBy,
            [FromQuery] bool ascending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? dueDateFrom = null,
            [FromQuery] DateTime? dueDateTo = null)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var tasks = await _taskService.GetByUserAsync(
                userId,
                categoryId,
                searchTitle,
                isCompleted,
                sortBy,
                ascending,
                page,
                pageSize,
                dueDateFrom,
                dueDateTo);

            return Ok(tasks);
        }
        [Authorize]
        [HttpPost("add-task")]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto, Guid? userId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = GetUserIdFromToken();
            var userRole = GetUserRoleFromToken();
            Guid targetUserId;
            if (userRole == "Admin" && userId.HasValue)
            {
                targetUserId = userId.Value;
            }
            else
            {
                targetUserId = currentUserId;
            }

            if (targetUserId == null) return Unauthorized();
            var task = await _taskService.CreateAsync(targetUserId, dto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id,TaskUpdateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateAsync(userId, id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var deleted = await _taskService.DeleteAsync(userId, id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpDelete("completed")]
        public async Task<IActionResult> DeleteCompletedTasks()
        {
            var userId = GetUserIdFromToken();

            var deletedCount = await _taskService.DeleteCompletedTasksAsync(userId);

            if (deletedCount == 0)
                return NotFound(new { message = "Tamamlanmış görev bulunamadı." });

            return Ok(new { message = $"{deletedCount} tamamlanmış görev silindi." });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("Admin-only")]
        public async Task<IActionResult> GetAll(
          [FromQuery] Guid? userId = null,
          [FromQuery] string? searchTitle = null,
          [FromQuery] Guid? categoryId = null,
          [FromQuery] bool? isCompleted = null,
          [FromQuery] string? sortBy = null,
          [FromQuery] bool ascending = true,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] DateTime? dueDateFrom = null,
          [FromQuery] DateTime? dueDateTo = null)
        {
            var tasks = await _taskService.GetAllAsync(
                 userId,
                 categoryId,
                 searchTitle,
                 isCompleted,
                 sortBy,
                 ascending,
                 page,
                 pageSize,
                 dueDateFrom,
                 dueDateTo);

            return Ok(tasks);
        }
        [HttpDelete("admin/{taskId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteTask(Guid taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var result = await _taskService.DeleteAsync(task.UserId, taskId);
            if (!result) return NotFound();

            return Ok(new { message = "Task deleted by Admin." });
        }
        [HttpPut("admin/{taskId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdateTask(Guid taskId, TaskUpdateDto dto)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var updatedTask = await _taskService.UpdateAsync(task.UserId, taskId, dto);
            if (updatedTask == null) return NotFound();

            return Ok(updatedTask);
        }

    }
}
