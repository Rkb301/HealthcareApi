using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Services;

public class AppointmentService: IAppointmentService
{
    private readonly IAppointmentRepository _repo;
    private readonly LuceneAppointmentIndexService _lucene;
    private readonly AssignmentDbContext _ctx;

    public AppointmentService(
        IAppointmentRepository repo,
        LuceneAppointmentIndexService lucene,
        AssignmentDbContext ctx)
    {
        _repo   = repo;
        _lucene = lucene;
        _ctx    = ctx;
    }

    public async Task<Appointment> AddAppointment(Appointment a)
    {
        a.CreatedAt  = DateTime.UtcNow;
        a.ModifiedAt = DateTime.UtcNow;
        var ret = await _repo.AddAsync(a);
        _lucene.IndexAppointment(ret);
        return ret;
    }

    public async Task<bool> SoftDeleteAppointment(int id)
    {
        var a = await _repo.GetByIdAsync(id);
        if (a == null) return false;
        a.isActive    = false;
        a.ModifiedAt  = DateTime.UtcNow;
        await _repo.UpdateAsync(a);
        _lucene.IndexAppointment(a);
        return true;
    }

    public async Task UpdateAppointment(int id, JsonPatchDocument patch)
    {
        var a = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
        patch.ApplyTo(a);
        a.ModifiedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(a);
        _lucene.IndexAppointment(a);
    }

    public async Task<Appointment?> GetAppointmentById(int id) =>
        await _repo.GetByIdAsync(id);

    public async Task<PagedResult<AppointmentWithNamesDTO>> SearchAppointments(AppointmentQueryParams qp)
    {
        if (string.IsNullOrWhiteSpace(qp.Query))
        {
            var query = _repo.GetBaseQuery();
            return await query
                .Select(a => new AppointmentWithNamesDTO
                {
                    AppointmentID   = a.AppointmentID,
                    PatientName     = a.Patient.FirstName + " " + a.Patient.LastName,
                    DoctorName      = a.Doctor.FirstName  + " " + a.Doctor.LastName,
                    AppointmentDate = a.AppointmentDate,
                    Reason          = a.Reason,
                    Status          = a.Status,
                    Notes           = a.Notes
                })
                .GetPagedResultAsync(qp.pageNumber, qp.pageSize);
        }

        var lucRes = _lucene.Search(qp.Query, qp.pageNumber, qp.pageSize, qp.Sort?.FirstOrDefault(), qp.Order);
        foreach (var dto in lucRes.Data)
        {
            var a = await _ctx.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .FirstAsync(x => x.AppointmentID == dto.AppointmentID);
            dto.PatientName = a.Patient.FirstName + " " + a.Patient.LastName;
            dto.DoctorName  = a.Doctor.FirstName  + " " + a.Doctor.LastName;
        }
        return lucRes;
    }
}
