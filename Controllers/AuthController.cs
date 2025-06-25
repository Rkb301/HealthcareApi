using HealthcareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

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
            Username = req.Username,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(req.Password, 13),
            Role = req.Role,
            CreatedAt = DateTime.Now
        };
#pragma warning restore CS8601 // Possible null reference assignment.

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered: {Email}", req.Email);
        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var user = _context.Users.SingleOrDefault(u => u.Email == req.Email);
        Console.Write("req:", req.Password, req.Email);
        _logger.LogInformation(req.ToString());

        if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", req.Email);
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

        _logger.LogInformation("User logged in: {Email}", req.Email);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}