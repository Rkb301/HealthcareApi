using Azure;
using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<Doctor>>> GetDoctors()
    {
        _logger.LogInformation("Fetching all doctors");
        try
        {
            var doctors = await _doctorService.GetAllDoctors();
            _logger.LogInformation("Returned {count} doctors", doctors.Count);
            return doctors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Doctors");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Doctor>> GetDoctor(int id)
    {
        _logger.LogInformation("Fetching docor with ID {id}", id);
        try
        {
            var doctor = await _doctorService.GetDoctorById(id);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor with ID {id} not found", id);
                return NotFound();
            }
            return doctor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching doctor with ID {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
    {
        _logger.LogInformation("Creating new doctor");
        try
        {
            var createdDoctor = await _doctorService.AddDoctor(doctor);
            _logger.LogInformation("Doctor created with ID {id}", createdDoctor.UserID);
            return CreatedAtAction(
                nameof(GetDoctor),
                new { id = createdDoctor.UserID },
                createdDoctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchDoctor(int id, [FromBody] JsonPatchDocument<Doctor> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogWarning("Patch document is null");
            return BadRequest();
        }

        try
        {
            await _doctorService.UpdateDoctor(id, patchDoc);
            _logger.LogInformation("Doctor {DoctorId} updated successfully", id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Doctor with ID {id} not found", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor {DoctorId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        _logger.LogInformation("Deleting doctor with ID {id}", id);
        try
        {
            var result = await _doctorService.SoftDeleteDoctor(id);
            if (!result)
            {
                _logger.LogWarning("Doctor with ID {id} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Doctor with ID {id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Doctor>>> GetWithParams([FromQuery] DoctorQueryParams param)
    {
        _logger.LogInformation("Searching doctors with params {@Params}", param);
        try
        {
            var result = await _doctorService.SearchDoctors(param);
            _logger.LogInformation("Found {Count} doctors matching search", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors with params {@Params}", param);
            return StatusCode(500, "Internal server error");
        }
    }
}