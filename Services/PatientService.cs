using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace HealthcareApi.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;
    private readonly ILogger<PatientService> _logger;
    private readonly LucenePatientIndexService _lucene;

    public PatientService(
        IPatientRepository repository,
        ILogger<PatientService> logger,
        LucenePatientIndexService lucene)
    {
        _repository = repository;
        _logger = logger;
        _lucene = lucene;
    }

    public async Task<List<Patient>> GetAllPatients()
    {
        return await _repository.GetBaseQuery().ToListAsync();
    }

    public async Task<Patient> GetPatientById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Patient> AddPatient(Patient patient)
    {
        patient.UserID = 1;                 // ONLY UNTIL NO AUTH
        patient.CreatedAt = DateTime.UtcNow;
        patient.ModifiedAt = DateTime.UtcNow;
        var returnee = await _repository.AddAsync(patient);
        _lucene.IndexPatient(patient);
        return returnee;
    }

    public async Task UpdatePatient(int id, JsonPatchDocument<Patient> patchDoc)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient == null) throw new NotFoundException();

        patchDoc.ApplyTo(patient);
        patient.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(patient);
        _lucene.IndexPatient(patient);
    }

    public async Task<bool> SoftDeletePatient(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient == null) return false;

        patient.isActive = false;
        patient.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(patient);

        _lucene.IndexPatient(patient);

        return true;
    }

    public async Task<List<UpcomingAppointmentDTO>> GetUpcomingAppointmentsAsync(int? id, string? status)
    {
        return await _repository.GetUpcomingAppointmentsAsync(id, status);
    }

    public async Task<PagedResult<Patient>> SearchPatients(PatientQueryParams param)
    {
        var query = _repository.GetBaseQuery();

        if (param.UID?.Any() == true)
            query = query.Where(p => param.UID.Contains((int)p.UserID));

        if (param.FirstName?.Any() == true)
            query = query.Where(p => param.FirstName.Contains(p.FirstName));

        if (param.LastName?.Any() == true)
            query = query.Where(p => param.LastName.Contains(p.LastName));

        if (param.Dob?.Any() == true)
            query = query.Where(p => param.Dob.Contains((DateOnly)p.DateOfBirth));

        if (param.Phone?.Any() == true)
            query = query.Where(p => param.Phone.Contains(p.ContactNumber));

        if (param.Sort?.Any() == true)
        {
            query = param.Sort.Aggregate(
                (IOrderedQueryable<Patient>)query.OrderBy(GetSortExpression(param.Sort.First(), param.Order)),
                (current, sortField) => current.ThenBy(GetSortExpression(sortField, param.Order))
            );
        }

        return await query.GetPagedResultAsync(param.pageNumber, param.pageSize);
    }

    private Expression<Func<Patient, object>> GetSortExpression(string field, string order)
    {
        return field.ToLower() switch
        {
            "userid" => p => p.UserID,
            "firstname" => p => p.FirstName,
            "lastname" => p => p.LastName,
            "dob" => p => p.DateOfBirth,
            "phone" => p => p.ContactNumber,
            _ => p => p.UserID
        };
    }
}
