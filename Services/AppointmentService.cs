using System.Linq.Expressions;
using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;
    public readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository repository,
        ILogger<AppointmentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Appointment> AddAppointment(Appointment appointment)
    {
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.ModifiedAt = DateTime.UtcNow;
        return await _repository.AddAsync(appointment);
    }

    public async Task<List<Appointment>> GetAllAppointments()
    {
        return await _repository.GetBaseQuery().ToListAsync();
    }

    public async Task<Appointment> GetAppointmentById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> SoftDeleteAppointment(int id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment == null) return false;

        appointment.isActive = false;
        appointment.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(appointment);
        return true;
    }

    public async Task UpdateAppointment(int id, JsonPatchDocument<Appointment> patchDoc)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment == null) throw new NotFoundException();

        patchDoc.ApplyTo(appointment);
        appointment.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(appointment);
    }

    public async Task<PagedResult<Appointment>> SearchAppointments(AppointmentQueryParams param)
    {
        var query = _repository.GetBaseQuery();

        if (param.PatientID?.Any() == true)
        {
            query = query.Where(a => param.PatientID.Contains(a.PatientID));
        }

        if (param.DoctorID?.Any() == true)
        {
            query = query.Where(a => param.DoctorID.Contains(a.DoctorID));
        }

        if (param.AppointmentDate?.Any() == true)
        {
            query = query.Where(a => param.AppointmentDate.Contains(a.AppointmentDate));
        }

        if (param.Status?.Any() == true)
        {
            query = query.Where(a => param.Status.Contains(a.Status));
        }

        if (param.CreatedAt?.Any() == true)
        {
            query = query.Where(a => param.CreatedAt.Contains(a.CreatedAt));
        }

        if (param.ModifiedAt?.Any() == true)
        {
            query = query.Where(a => param.ModifiedAt.Contains(a.ModifiedAt));
        }

        if (param.isActive?.Any() == true)
        {
            query = query.Where(a => param.isActive.Contains(a.isActive));
        }


        // Sorting
        if (param.Sort?.Any() == true)
        {
            query = param.Sort.Aggregate(
                (IOrderedQueryable<Appointment>)query.OrderBy(GetSortExpression(param.Sort.First(), param.Order)),
                (current, sortField) => current.ThenBy(GetSortExpression(sortField, param.Order))
            );
        }

        return await query.GetPagedResultAsync(param.pageNumber, param.pageSize);
    }

    private Expression<Func<Appointment, object>> GetSortExpression(string field, string order)
    {
        return field.ToLower() switch
        {
            "patientid" => a => a.PatientID,
            "doctorid" => a => a.DoctorID,
            "appointmentdate" => a => a.AppointmentDate,
            "status" => a => a.Status,
            "createdat" => a => a.CreatedAt,
            "modifiedat" => a => a.ModifiedAt,
            "isactive" => a => a.isActive,
            _ => a => a.PatientID
        };
    }
}