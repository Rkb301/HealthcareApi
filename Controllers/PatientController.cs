using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientController> _logger;
    private readonly LucenePatientIndexService _luceneIndexService;

    public PatientController(
        IPatientService patientService,
        ILogger<PatientController> logger,
        LucenePatientIndexService lucene)
    {
        _patientService = patientService;
        _logger = logger;
        _luceneIndexService = lucene;
    }

    // [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<Patient>>> GetPatients()
    {
        _logger.LogInformation("Fetching all patients");
        try
        {
            var patients = await _patientService.GetAllPatients();
            _logger.LogInformation("Returned {Count} patients", patients.Count);
            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients");
            return StatusCode(500, "Internal server error");
        }
    }

    // [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        _logger.LogInformation("Fetching patient with ID {id}", id);
        try
        {
            var patient = await _patientService.GetPatientById(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient with ID {id} not found", id);
                return NotFound();
            }
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient with ID {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient(Patient patient)
    {
        _logger.LogInformation("Creating new patient");
        try
        {
            var createdPatient = await _patientService.AddPatient(patient);
            _logger.LogInformation("Patient created with ID {id}", createdPatient.PatientID);
            return CreatedAtAction(
                nameof(GetPatient),
                new { id = createdPatient.PatientID },
                createdPatient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, "Internal server error");
        }
    }

    // [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchPatient(int id, [FromBody] JsonPatchDocument<Patient> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogWarning("Patch document is null");
            return BadRequest();
        }

        try
        {
            await _patientService.UpdatePatient(id, patchDoc);
            _logger.LogInformation("Patient {PatientId} updated successfully", id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Patient with ID {id} not found", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {PatientId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        _logger.LogInformation("Deleting patient with ID {Id}", id);
        try
        {
            var result = await _patientService.SoftDeletePatient(id);
            if (!result)
            {
                _logger.LogWarning("Patient with ID {Id} not found for deletion", id);
                return NotFound();
            }
            _logger.LogInformation("Patient with ID {Id} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<Patient>>> GetWithParams([FromQuery] PatientQueryParams param)
    {
        _logger.LogInformation("Searching patients with params {@Params}", param);
        try
        {
            var result = await _patientService.SearchPatients(param);
            _logger.LogInformation("Found {Count} patients matching search", result.TotalCount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with params {@Params}", param);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("search-lucene")]
    public ActionResult<PagedResult<Patient>> SearchLucene(
        [FromQuery] string? query, 
        [FromQuery] string? sort,
        [FromQuery] string? order,
        int pageNumber = 1, 
        int pageSize = 10)
    {
        _logger.LogInformation("Lucene search for query '{query}' with sort '{sort}' order '{order}'", query, sort, order);
        var result = _luceneIndexService.Search(query, pageNumber, pageSize, sort, order);
        
        return Ok(result);
    }

}
