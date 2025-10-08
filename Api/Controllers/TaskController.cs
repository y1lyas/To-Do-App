using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;
using ToDoApp.Services;
using ToDoApp.Services.Auth;


namespace ToDoApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly ITaskAuthService _taskAuthService;

        public TaskController(ITaskService taskService, ITaskAuthService taskAuthService)
        {
            _taskService = taskService;
            _taskAuthService = taskAuthService;
        }

        [Authorize]
        [HttpGet("by-user")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTask(
            [FromQuery] string? searchTitle,
            [FromQuery] Guid? categoryId,
            [FromQuery] TaskAssignmentStatus? status,
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
                requesterId: userId,
                categoryId,
                searchTitle,
                status,
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
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> CreateTask([FromBody] TaskCreateDto dto, Guid? userId = null)
        {

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
        [HttpPut("task-by-{id}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> UpdateTask(Guid id,TaskUpdateDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateAsync(userId, id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("task-by{id}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> DeleteTask(Guid id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var deleted = await _taskService.DeleteAsync(userId, id);
            if (!deleted) return NotFound();

            return NoContent();
        }
        [Authorize(Roles = "Captain")]
        [HttpPost("{taskId}/assign/{userId}")]
        public async Task<ActionResult<TaskResponseDto>> AssignUserToTask(Guid taskId, Guid userId)
        {
            var currentUserId = GetUserIdFromToken();

            if (!await _taskAuthService.CanModifyTaskAsync(taskId, currentUserId))
                return Unauthorized("You are not allowed to modify this task.");


            var task = await _taskService.AssignUserToTaskAsync(taskId, userId);
            if (task == null) return NotFound();
            return Ok(task);
        }
        [Authorize(Roles = "Captain")]
        [HttpDelete("{taskId}/assign/{userId}")]
        public async Task<ActionResult<TaskResponseDto>> RemoveUserFromTask(Guid taskId, Guid userId)
        {
            var currentUserId = GetUserIdFromToken();

            if (!await _taskAuthService.CanModifyTaskAsync(taskId, currentUserId))
                return Unauthorized("You are not allowed to modify this task.");

            var task = await _taskService.RemoveUserFromTaskAsync(taskId, userId);
            if (task == null) return NotFound();
            return Ok(task);
        }
        [Authorize]
        [HttpPut("assignments/{taskId}/status")]
        public async Task<IActionResult> UpdateAssignmentStatus(Guid taskId, [FromBody] UpdateAssignmentStatusDto dto)
        {
            var userId = GetUserIdFromToken();

            var task = await _taskService.UpdateAssignmentStatusAsync(userId, taskId, dto.Status);
            if (task == null) 
                return NotFound();


            return Ok(task);
        }

        [HttpGet("Captain-Only")]
        [Authorize(Roles = "Captain")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetSubordinatesTasks(
            [FromQuery] Guid? userId,
            [FromQuery] Guid? categoryId,
            [FromQuery] TaskAssignmentStatus? status,
            [FromQuery] string? searchTitle,
            [FromQuery] string? sortBy,
            [FromQuery] bool ascending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? dueDateFrom = null,
            [FromQuery] DateTime? dueDateTo = null)
        {
            var captainId = GetUserIdFromToken();

            var tasks = await _taskService.GetSubordinatesTasksAsync(
                captainId,
                userId,
                categoryId,
                searchTitle,
                status,
                sortBy,
                ascending,
                page,
                pageSize,
                dueDateFrom,
                dueDateTo
            );

            return Ok(tasks);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin-only")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll(
          [FromQuery] Guid? userId = null,
          [FromQuery] string? searchTitle = null,
          [FromQuery] Guid? categoryId = null,
          [FromQuery] TaskAssignmentStatus? status = null,
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
                 status,
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
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> AdminDeleteTask(Guid taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var result = await _taskService.DeleteAsync(task.CreatedById, taskId);
            if (!result) return NotFound();

            return Ok(new { message = "Task deleted by Admin." });
        }

        [HttpPut("admin/{taskId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> AdminUpdateTask(Guid taskId, TaskUpdateDto dto)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null) return NotFound();

            var updatedTask = await _taskService.UpdateAsync(task.CreatedById, taskId, dto);
            if (updatedTask == null) return NotFound();

            return Ok(updatedTask);
        }

    [HttpPost("Captain-Only-assign")]
    [Authorize(Roles = "Captain")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> CreateTaskForSubordinates([FromBody] TaskCreateDto dto, [FromQuery] List<Guid>? subordinateIds)
    {
            var captainId = GetUserIdFromToken();
        var result = await _taskService.CreateTaskForSubordinatesAsync(captainId, dto, subordinateIds);
        if (result == null) return BadRequest("Invalid subordinate IDs.");
        return Ok(result);
    } 


    [HttpPut("update-subordinate/{taskId}")]
    [Authorize(Roles = "Captain")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> UpdateSubordinateTask(Guid taskId, [FromBody] TaskUpdateDto dto)
    {
        var captainId = GetUserIdFromToken();
        var result = await _taskService.UpdateSubordinateTaskAsync(captainId, taskId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
        [Authorize(Roles = "Captain")]
        [HttpDelete("archive-completed")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> ArchiveCompletedTasks()
        {
            var userId = GetUserIdFromToken();

            var archivedCount = await _taskService.ArchiveCompletedTasksAsync(userId);

            if (archivedCount == 0)
                return NotFound(new { message = "No completed tasks available for archiving." });

            return Ok(new { message = $"{archivedCount} completed task(s) archived successfully." });
        }
    }
}
