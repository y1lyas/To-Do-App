using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoApp.Domain;
using ToDoApp.DTOs;
using ToDoApp.Infrastructure;
using ToDoApp.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;


    public AuthController(AppDbContext context, ITokenService tokenService, IAuthService authService)
    {
        _context = context;
        _tokenService = tokenService;
        _authService = authService;

    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto dto)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginDto dto)
    {
        try
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
    {
        try
        {
            var tokenResponse = await _authService.RefreshTokenAsync(request);
            if (tokenResponse is null || tokenResponse.AccessToken is null || tokenResponse.RefreshToken is null)
                return Unauthorized("Refresh or Access token is null!");
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndpoint()
    {
        Console.WriteLine("You are authenticated!");
        return Ok("You are authenticated!");
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-data")]
    public IActionResult GetAdminData()
    {
        Console.WriteLine("You are an admin!");
        return Ok("You are an admin!");
    }
}
