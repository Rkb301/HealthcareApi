using HealthcareApi.Models;
using HealthcareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _svc;

    public AppointmentController(IAppointmentService svc) => _svc = svc;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentWithNamesDTO>>> GetAll(
        [FromQuery] AppointmentQueryParams qp)
    {
        return Ok(await _svc.SearchAppointments(qp));
    }

    [Authorize(Roles = "Admin, Doctor, Patient")]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        return (await _svc.GetAppointmentById(id)) is Appointment a ? Ok(a) : NotFound();
    }

    [Authorize(Roles = "Admin, Patient")]
    [HttpPost]
    public async Task<IActionResult> Post(Appointment a)
    {
        var created = await _svc.AddAppointment(a);
        return CreatedAtAction(nameof(Get), new { id = created.AppointmentID }, created);
    }

    [Authorize(Roles = "Admin, Doctor")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument patch)
    {
        if (patch is null) return BadRequest();
        await _svc.UpdateAppointment(id, patch);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _svc.SoftDeleteAppointment(id) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin, Doctor, Patient")]
    [HttpGet("search-lucene")]
    public async Task<IActionResult> SearchLucene(
        [FromQuery] AppointmentQueryParams qp)
    {
        return Ok(await _svc.SearchAppointments(qp));
    }
}
