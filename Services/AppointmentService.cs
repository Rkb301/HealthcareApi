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
            // Non-search path - include patient/doctor names efficiently
            var query = _repo.GetBaseQuery()
                .Include(a => a.Patient)
                .Include(a => a.Doctor);
                
            return await query
                .Select(a => new AppointmentWithNamesDTO
                {
                    AppointmentID = a.AppointmentID,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    AppointmentDate = a.AppointmentDate,
                    Reason = a.Reason,
                    Status = a.Status,
                    Notes = a.Notes
                })
                .GetPagedResultAsync(qp.pageNumber, qp.pageSize);
        }
    
        // names included in search results directly
        var lucRes = _lucene.Search(qp.Query, qp.pageNumber, qp.pageSize, qp.Sort?.FirstOrDefault(), qp.Order);
        
        // Get appointment IDs for efficient batch loading
        var appointmentIds = lucRes.Data.Select(dto => dto.AppointmentID).ToList();
        var appointmentsWithNames = await _ctx.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => appointmentIds.Contains(a.AppointmentID))
            .ToDictionaryAsync(a => a.AppointmentID);
    
        // Update DTOs with names efficiently
        foreach (var dto in lucRes.Data)
        {
            if (appointmentsWithNames.TryGetValue(dto.AppointmentID, out var appointment))
            {
                dto.PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}";
                dto.DoctorName = $"{appointment.Doctor.FirstName} {appointment.Doctor.LastName}";
            }
        }
        
        return lucRes;
    }

}
