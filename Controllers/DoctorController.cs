using HealthcareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly AssignmentDbContext _context;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(AssignmentDbContext context, ILogger<DoctorController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
    {
        _logger.LogInformation("Fetching all doctors");
        try
        {
            var doctors = await _context.Doctors.ToListAsync();
            _logger.LogInformation("Returned {Count} doctors", doctors.Count);
            return doctors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctors");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Doctor>> GetDoctor(int id)
    {
        _logger.LogInformation("Fetching doctor with ID {Id}", id);
        try
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                _logger.LogWarning("Doctor with ID {Id} not found", id);
                return NotFound();
            }

            return doctor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
    {
        _logger.LogInformation("Creating new doctor");
        try
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Doctor created with ID {Id}", doctor.DoctorID);
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorID }, doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutDoctor(int id, Doctor doctor)
    {
        if (id != doctor.DoctorID)
        {
            _logger.LogWarning("Doctor ID mismatch: {Id} != {DoctorId}", id, doctor.DoctorID);
            return BadRequest();
        }

        _context.Entry(doctor).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Doctor {DoctorId} updated successfully", id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating doctor {DoctorId}", id);
            if (!DoctorExists(id))
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
            _logger.LogError(ex, "Unexpected error updating doctor {DoctorId}", id);
            return StatusCode(500, "Internal server error");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        _logger.LogInformation("Deleting doctor with ID {Id}", id);
        try
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor with ID {Id} not found for deletion", id);
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Doctor with ID {Id} deleted", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private bool DoctorExists(int id)
    {
        return _context.Doctors.Any(e => e.DoctorID == id);
    }
}
