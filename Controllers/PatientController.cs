using HealthcareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly AssignmentDbContext _context;
    private readonly ILogger<PatientController> _logger;

    public PatientController(AssignmentDbContext context, ILogger<PatientController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
    {
        _logger.LogInformation("Fetching all patients");
        try
        {
            var patients = await _context.Patients.ToListAsync();
            _logger.LogInformation("Returned {Count} patients", patients.Count);
            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        _logger.LogInformation("Fetching patient with ID {Id}", id);
        try
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                _logger.LogWarning("Patient with ID {Id} not found", id);
                return NotFound();
            }

            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient(Patient patient)
    {
        _logger.LogInformation("Creating new patient");
        try
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Patient created with ID {Id}", patient.PatientID);
            return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientID }, patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPatient(int id, Patient patient)
    {
        if (id != patient.PatientID)
        {
            _logger.LogWarning("Patient ID mismatch: {Id} != {PatientId}", id, patient.PatientID);
            return BadRequest();
        }

        _context.Entry(patient).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Patient {PatientId} updated successfully", id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating patient {PatientId}", id);
            if (!PatientExists(id))
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
            _logger.LogError(ex, "Unexpected error updating patient {PatientId}", id);
            return StatusCode(500, "Internal server error");
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        _logger.LogInformation("Deleting patient with ID {Id}", id);
        try
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient with ID {Id} not found for deletion", id);
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Patient with ID {Id} deleted", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private bool PatientExists(int id)
    {
        return _context.Patients.Any(e => e.PatientID == id);
    }
}
