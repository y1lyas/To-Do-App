using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApp.DTOs.Auth;
using ToDoApp.Infrastructure;
using ToDoApp.Models;
using ToDoApp.Models.Auth;

namespace ToDoApp.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                throw new Exception("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var userRole = new UserRole
            {
                User = user,
                RoleId = defaultRole.Id 
            };

            _context.UserRoles.Add(userRole);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid Username or Password.");
            
            return await CreateTokenResponse(user);
        }
        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto { AccessToken = _tokenService.CreateToken(user), RefreshToken = await _tokenService.GenerateAndSaveRefreshTokenAsync(user) };
        }
        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await _tokenService.ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user == null)
                return null;

            return await CreateTokenResponse(user);
        }

    }
}
