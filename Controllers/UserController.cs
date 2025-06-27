using Azure;
using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;


    public UserController(
        IUserService userService,
        ILogger<UserController> logger
    )
    {
        _userService = userService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        _logger.LogInformation("Fetching all users");
        try
        {
            var users = await _userService.GetAllUsers();
            _logger.LogInformation("Returned {Count} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        _logger.LogInformation("Fetching user with ID {id}", id);
        try
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {id} not found", id);
                return NotFound();
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID {id}", id);
            return StatusCode(500, "Internal server error");
        }

    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        _logger.LogInformation("Creating new user");
        try
        {
            var createdUser = await _userService.AddUser(user);
            _logger.LogInformation("User created with ID {id}", createdUser.UserID);
            return CreatedAtAction(
                nameof(GetUser),
                new { id = createdUser.UserID },
                createdUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogWarning("Patch document is null");
            return BadRequest();
        }

        try
        {
            await _userService.UpdateUser(id, patchDoc);
            _logger.LogInformation("User {UserId} updated succesfully", id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("User with ID {id} not found", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID {id}", id);
        try
        {
            var result = await _userService.SoftDeleteUser(id);
            if (!result)
            {
                _logger.LogWarning("User with ID {id} not found for deletion", id);
                return NotFound();
            }
            _logger.LogInformation("User with ID {id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting User with ID {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<User>>> GetWithParams([FromQuery] UserQueryParams param)
    {
        _logger.LogInformation("Searching users with params {@Params}", param);
        try
        {
            var result = await _userService.SearchUsers(param);
            _logger.LogInformation("Found {count} users among search", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with params {@Params}", param);
            return StatusCode(500, "Internal server error");
        }
    }
}