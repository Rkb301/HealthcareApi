using HealthcareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AssignmentDbContext _context;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(AssignmentDbContext context, ILogger<AppointmentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
    {
        _logger.LogInformation("Fetching all appointments");
        try
        {
            var appointments = await _context.Appointments.ToListAsync();
            _logger.LogInformation("Returned {Count} appointments", appointments.Count);
            return appointments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointments");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        _logger.LogInformation("Fetching appointment with ID {Id}", id);
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {Id} not found", id);
                return NotFound();
            }

            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointment with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
    {
        _logger.LogInformation("Creating new appointment");
        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Appointment created with ID {Id}", appointment.AppointmentID);
            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentID }, appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentID)
        {
            _logger.LogWarning("Appointment ID mismatch: {Id} != {AppointmentId}", id, appointment.AppointmentID);
            return BadRequest();
        }

        _context.Entry(appointment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Appointment {AppointmentId} updated successfully", id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating appointment {AppointmentId}", id);
            if (!AppointmentExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating appointment {AppointmentId}", id);
            return StatusCode(500, "Internal server error");
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        _logger.LogInformation("Deleting appointment with ID {Id}", id);
        try
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment with ID {Id} not found for deletion", id);
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Appointment with ID {Id} deleted", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private bool AppointmentExists(int id)
    {
        return _context.Appointments.Any(e => e.AppointmentID == id);
    }
}
