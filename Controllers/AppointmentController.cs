using Azure;
using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<Appointment>>> GetAppointments()
    {
        _logger.LogInformation("Fetching all appointments");
        try
        {
            var appointments = await _appointmentService.GetAllAppointments();
            _logger.LogInformation("Returned {Count} appointments", appointments.Count);
            return appointments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointments");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        _logger.LogInformation("Fetching appointment with ID {id}", id);
        try
        {
            var appointment = await _appointmentService.GetAppointmentById(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {id} not found", id);
                return NotFound();
            }
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment with ID {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
    {
        _logger.LogInformation("Creating new appointment");
        try
        {
            var createdAppointment = await _appointmentService.AddAppointment(appointment);
            _logger.LogInformation("Appointment created with ID {id}", createdAppointment.AppointmentID);
            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = createdAppointment.AppointmentID },
                createdAppointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchAppointment(int id, [FromBody] JsonPatchDocument<Appointment> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogWarning("Patch document is null");
            return BadRequest();
        }

        try
        {
            await _appointmentService.UpdateAppointment(id, patchDoc);
            _logger.LogInformation("Appointment {AppointmentId} updated succesfully", id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Appointment with ID {id} not found", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {AppointmentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        _logger.LogInformation("Deleting appointment with ID {id}", id);
        try
        {
            var result = await _appointmentService.SoftDeleteAppointment(id);
            if (!result)
            {
                _logger.LogWarning("Appointment with ID {id} not found for deletion", id);
                return NotFound();
            }
            _logger.LogInformation("Appointment with ID {id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Appointment>>> GetWithParams([FromQuery] AppointmentQueryParams param)
    {
        _logger.LogInformation("Searching appointments with params {@Params}", param);
        try
        {
            var result = await _appointmentService.SearchAppointments(param);
            _logger.LogInformation("Found {Count} appointments matching search", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching appointments with params {@Params}", param);
            return StatusCode(500, "Internal server error");
        }
    }
}
