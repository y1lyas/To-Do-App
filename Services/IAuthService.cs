using ToDoApp.DTOs;

namespace ToDoApp.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<TokenResponseDto> LoginAsync(LoginDto dto);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
