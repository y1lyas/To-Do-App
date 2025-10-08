using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToDoApp.Models.Auth;

namespace ToDoApp.Api.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID claim not found in token.");

            return Guid.Parse(userIdClaim.Value);
        }

        protected string GetUserRoleFromToken()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
                throw new UnauthorizedAccessException("Role claim not found in token.");

            return roleClaim.Value;
        }

        protected string? GetUserEmailFromToken()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }
        protected CurrentUserContext GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
                throw new UnauthorizedAccessException("Invalid JWT claims.");

            return new CurrentUserContext
            {
                Id = Guid.Parse(userIdClaim),
                Role = roleClaim,
                Email = emailClaim ?? string.Empty
            };
        }
    }   
}
