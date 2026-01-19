using LibraryApi.Data;
using LibraryApi.Domain;
using LibraryApi.Dtos.Auth;
using LibraryApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;

    public AuthController(
        AppDbContext db,
        ITokenService tokenService,
        ILogger<AuthController> logger,
        IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
        _config = config;
    }

    // Register a new user
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var username = (request.Username ?? "").Trim();

        _logger.LogInformation("POST /api/auth/register called. Username={Username}", username);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Registration failed due to missing username or password");
            return BadRequest(new { message = "Username and password are required." });
        }

        var usernameMin = _config.GetValue<int>("UserValidation:UsernameMinLength");
        var usernameMax = _config.GetValue<int>("UserValidation:UsernameMaxLength");
        var passwordMin = _config.GetValue<int>("UserValidation:PasswordMinLength");
        var passwordMax = _config.GetValue<int>("UserValidation:PasswordMaxLength");

        if (username.Length < usernameMin || username.Length > usernameMax)
        {
            _logger.LogWarning("Registration failed. Invalid username length: {Username}", username);
            return BadRequest(new { message = $"Username must be between {usernameMin} and {usernameMax} characters." });
        }

        if (request.Password.Length < passwordMin || request.Password.Length > passwordMax)
        {
            _logger.LogWarning("Registration failed. Invalid password length for Username={Username}", username);
            return BadRequest(new { message = $"Password must be between {passwordMin} and {passwordMax} characters." });
        }

        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists)
        {
            _logger.LogWarning("Registration failed. Username already exists: {Username}", username);
            return Conflict(new { message = "Username already exists." });
        }

        var user = new AppUser { Username = username };
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("User registered successfully. Username={Username}", username);

        return StatusCode(201, new { message = "Registered successfully." });
    }


    // Login user and return JWT token
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var username = request.Username.Trim();

        _logger.LogInformation("POST /api/auth/login called. Username={Username}", username);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            _logger.LogWarning("Login failed. User not found: {Username}", username);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var hasher = new PasswordHasher<AppUser>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed. Invalid password for Username={Username}", username);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = _tokenService.CreateToken(user);

        _logger.LogInformation("User logged in successfully. Username={Username}", username);

        return Ok(new AuthResponse(token));
    }
}
