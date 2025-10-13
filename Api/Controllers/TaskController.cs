using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using System.Threading.Tasks;
using ToDoApp.DTOs.Task;
using ToDoApp.Models;
using ToDoApp.Services;
using ToDoApp.Services.Auth;
using ToDoApp.Services.Authorization;


namespace ToDoApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly ICustomAuthorizationService _authorizationService;

        public TaskController(ITaskService taskService, ICustomAuthorizationService authorizationService)
        {
            _taskService = taskService;
            _authorizationService = authorizationService;
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
        [HttpGet("assignments")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetMyAssignments(
    [FromQuery] Guid? categoryId = null,
    [FromQuery] string? searchTitle = null,
    [FromQuery] TaskAssignmentStatus? status = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool ascending = true,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] DateTime? dueDateFrom = null,
    [FromQuery] DateTime? dueDateTo = null)
        {
            var currentUser = GetCurrentUser();
            var tasks = await _taskService.GetMyAssignmentsAsync(
                currentUser, categoryId, searchTitle, status,
                sortBy, ascending, page, pageSize, dueDateFrom, dueDateTo);
            return Ok(tasks);
        }

        [Authorize]
        [HttpPost("add-task")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> CreateTask([FromBody] TaskCreateDto dto)
        {

            var currentUser = GetCurrentUser();

            if (currentUser == null) return Unauthorized();
            var task = await _taskService.CreateAsync(currentUser, dto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [Authorize]
        [HttpPut("task-by-{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> UpdateTask(Guid taskId,TaskUpdateDto dto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            bool hasPermission = await _authorizationService.AuthorizeTaskAccessAsync(currentUser, taskId, TaskPermission.Update);
            if (!hasPermission)
                return Forbid();

            var result = await _taskService.UpdateAsync(currentUser, taskId, dto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("task-by{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> DeleteTask(Guid taskId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();
            bool hasPermission = await _authorizationService.AuthorizeTaskAccessAsync(currentUser, taskId, TaskPermission.Delete);
            if (!hasPermission)
                return Forbid();

            var result = await _taskService.DeleteAsync(currentUser, taskId);
            if (!result) return NotFound();

            return NoContent();
        }
        [Authorize(Roles = "Captain,Admin")]
        [HttpPost("{taskId}/assign/{userId}")]
        public async Task<ActionResult<TaskResponseDto>> AssignUserToTask(Guid taskId, Guid userId)
        {
            var currentUser = GetCurrentUser();   

                 bool hasPermission = await _authorizationService.AuthorizeTaskAccessAsync(currentUser, taskId, TaskPermission.Assign);
            if (!hasPermission)
                return Forbid();

            var task = await _taskService.AssignUserToTaskAsync(taskId, userId);
            if (task == null) return NotFound();
            return Ok(task);    
        }
        [Authorize(Roles = "Captain,Admin")]
        [HttpDelete("{taskId}/assign/{userId}")]
        public async Task<ActionResult<TaskResponseDto>> RemoveUserFromTask(Guid taskId, Guid userId)
        {
            var currentUser = GetCurrentUser();

            bool hasPermission = await _authorizationService.AuthorizeTaskAccessAsync(currentUser, taskId, TaskPermission.Unassign);
            if (!hasPermission)
                return Forbid();

            var task = await _taskService.RemoveUserFromTaskAsync(taskId, userId);
            if (task == null) return NotFound();
            return Ok(task);
        }
        [Authorize]
        [HttpPut("assignments/{taskId}/status")]
        public async Task<IActionResult> UpdateAssignmentStatus(Guid taskId, [FromBody] UpdateAssignmentStatusDto dto)
        {
            var currentUser = GetCurrentUser();
            bool hasPermission = await _authorizationService.AuthorizeTaskAccessAsync(currentUser, taskId, TaskPermission.UpdateStatus);
            if (!hasPermission)
                return Forbid();
            var task = await _taskService.UpdateAssignmentStatusAsync(currentUser, taskId, dto.Status);
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
    [HttpPost("Captain-Only-assign")]
    [Authorize(Roles = "Captain")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> CreateTaskForSubordinates([FromBody] TaskCreateDto dto, [FromQuery] List<Guid>? subordinateIds)
    {
        var captainId = GetUserIdFromToken();
        var result = await _taskService.CreateTaskForSubordinatesAsync(captainId, dto, subordinateIds);
        if (result == null) return BadRequest("Invalid subordinate IDs.");
        return Ok(result);
    } 

        [Authorize]
        [HttpDelete("archive-completed")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> ArchiveCompletedTasks()
        {
            var currentUser = GetCurrentUser();

            var archivedCount = await _taskService.ArchiveCompletedTasksAsync(currentUser);

            if (archivedCount == 0)
                return NotFound(new { message = "No completed tasks available for archiving." });

            return Ok(new { message = $"{archivedCount} completed task(s) archived successfully." });
        }
    }
}
