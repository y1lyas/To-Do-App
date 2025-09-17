using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ToDoApp.Domain;
using ToDoApp.Infrastructure;


namespace ToDoApp.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        public TokenService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string CreateToken(User user)
        {

            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var roles = _context.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Select(ur => ur.Role.Name)
                                .ToList();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
              issuer: _config["JwtSettings:Issuer"],
              audience: _config["JwtSettings:Audience"],
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
              signingCredentials: creds
          );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);  
            await _context.SaveChangesAsync();

            return (refreshToken);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
        public async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new Exception("Invalid or expired refresh token");
            return user;
        }
    }   
}
