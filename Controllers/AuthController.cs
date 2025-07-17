using HealthcareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System.Runtime.Intrinsics.Arm;
using NuGet.Protocol;
using static Microsoft.AspNetCore.Http.ISession;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AssignmentDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AssignmentDbContext context, IConfiguration config, ILogger<AuthController> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    private string GenerateRefreshToken()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] tokenData = new byte[64];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData);
    }

    private JwtSecurityToken GenerateAccessToken(User user)
    {
        _logger.LogInformation("Role: {Role}", user.Role);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("userid", user.UserID.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["AuthConfiguration:Key"] ?? throw new InvalidOperationException("JWT key not configured")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _config["AuthConfiguration:Issuer"],
            audience: _config["AuthConfiguration:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
    }

    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class RefreshRequest
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (_context.Users.Any(u => u.Email == req.Email))
        {
            _logger.LogWarning("Attempt to register with already used email: {Email}", req.Email);
            return BadRequest("Email already registered.");
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        var user = new User
        {
            UserID = Random.Shared.Next(0, 999999),
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(req.Password, 13),
            Role = req.Role,
            CreatedAt = DateTime.Now,
            ModifiedAt = DateTime.Now
        };
#pragma warning restore CS8601 // Possible null reference assignment.

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered: {Email}", req.Email);
        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var normalizedEmail = req.Email?.Trim().ToLower();
        var user = _context.Users
            .FirstOrDefault(u => u.Email.Trim().ToLower() == normalizedEmail);

        if (user == null) { return Unauthorized("null user"); }
        if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}, {Password}", req?.Email, req?.Password);
            return Unauthorized("Invalid credentials");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("userid", user.UserID.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

#pragma warning disable CS8604 // Possible null reference argument.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AuthConfiguration:Key"]));
#pragma warning restore CS8604 // Possible null reference argument.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["AuthConfiguration:Issuer"],
            audience: _config["AuthConfiguration:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        _logger.LogInformation("User logged in: {Email}", req?.Email);
        _logger.LogInformation("Role: {Role}", user.Role);


        var accessToken = GenerateAccessToken(user);
        _logger.LogInformation("claims of token: {claims}", claims[2]);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
            refreshToken = refreshToken,
            expiresIn = accessToken.ValidTo,
            role = user.Role,
            user.Email,
            user.Username,
            user.isActive
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrEmpty(req?.RefreshToken))
            return BadRequest();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == req.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized();

        var newRefreshToken = GenerateRefreshToken();
        var newAccessToken = GenerateAccessToken(user);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            refreshToken = newRefreshToken,
            expiresIn = newAccessToken.ValidTo
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst("userid")?.Value;
        if (!int.TryParse(userId, out int id))
            return BadRequest("Invalid user");
    
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();
        }
    
        return Ok(new { message = "Logged out successfully" });
    }

}
