using ToDoApp.Domain;

namespace ToDoApp.Services.Auth
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string GenerateRefreshToken();
        Task<string> GenerateAndSaveRefreshTokenAsync(User user);
        Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
    }
}
