using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _svc;

    public DoctorController(IDoctorService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DoctorQueryParams qp) =>
        Ok(await _svc.SearchDoctors(qp));

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
        Ok(await _svc.SearchDoctors(qp));
}
