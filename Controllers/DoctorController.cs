using HealthcareApi.Models;
using HealthcareApi.Repositories;
using HealthcareApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _svc;
    private readonly IDoctorRepository _repo;

    public DoctorController(IDoctorService svc, IDoctorRepository repo) {
        _svc = svc;
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DoctorQueryParams qp) =>
        Ok(await _svc.SearchDoctorsLucene(qp));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        (await _svc.GetDoctorById(id)) is Doctor d ? Ok(d) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Post(Doctor d)
    {
        var created = await _svc.AddDoctor(d);
        return CreatedAtAction(nameof(Get), new { id = created.DoctorID }, created);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument patch)
    {
        if (patch is null) return BadRequest();
        await _svc.UpdateDoctor(id, patch);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _svc.SoftDeleteDoctor(id) ? NoContent() : NotFound();

    [HttpGet("search-lucene")]
    public async Task<IActionResult> SearchLucene(
        [FromQuery] DoctorQueryParams qp) =>
        Ok(await _svc.SearchDoctorsLucene(qp));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] DoctorQueryParams qp)
    {
        return Ok(await _svc.SearchDoctors(qp));
    }

    [HttpGet("proc")]
    public async Task<IActionResult> PresentAppointments([FromQuery] int? id, [FromQuery] string? filter)
    {
        return Ok(await _svc.GetPresentAppointments(id, filter));
    }
}
