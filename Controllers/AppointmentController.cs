using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _svc;

    public AppointmentController(IAppointmentService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentWithNamesDTO>>> GetAll(
        [FromQuery] AppointmentQueryParams qp) =>
        Ok(await _svc.SearchAppointments(qp));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        (await _svc.GetAppointmentById(id)) is Appointment a ? Ok(a) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Post(Appointment a)
    {
        var created = await _svc.AddAppointment(a);
        return CreatedAtAction(nameof(Get), new { id = created.AppointmentID }, created);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument patch)
    {
        if (patch is null) return BadRequest();
        await _svc.UpdateAppointment(id, patch);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _svc.SoftDeleteAppointment(id) ? NoContent() : NotFound();

    [HttpGet("search-lucene")]
    public async Task<IActionResult> SearchLucene(
        [FromQuery] AppointmentQueryParams qp) =>
        Ok(await _svc.SearchAppointments(qp));
}
