using System.Linq.Expressions;
using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Services;

public class DoctorService : IDoctorService
{

    private readonly IDoctorRepository _repository;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IDoctorRepository repository,
        ILogger<DoctorService> logger
    )
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Doctor>> GetAllDoctors()
    {
        return await _repository.GetBaseQuery().ToListAsync();
    }

    public async Task<Doctor> GetDoctorById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Doctor> AddDoctor(Doctor doctor)
    {
        doctor.CreatedAt = DateTime.UtcNow;
        doctor.ModifiedAt = DateTime.UtcNow;
        return await _repository.AddAsync(doctor);
    }

    public async Task UpdateDoctor(int id, JsonPatchDocument<Doctor> patchDoc)
    {
        var doctor = await _repository.GetByIdAsync(id);
        if (doctor == null) throw new NotFoundException();

        patchDoc.ApplyTo(doctor);
        doctor.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(doctor);
    }

    public async Task<bool> SoftDeleteDoctor(int id)
    {
        var doctor = await _repository.GetByIdAsync(id);
        if (doctor == null) return false;

        doctor.isActive = false;
        doctor.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(doctor);
        return true;
    }

    public async Task<PagedResult<Doctor>> SearchDoctors(DoctorQueryParams param)
    {
        var query = _repository.GetBaseQuery();
        
        if (param.FirstName?.Any() == true)
        {
            query = query.Where(d => param.FirstName.Contains(d.FirstName));
        }

        if (param.LastName?.Any() == true)
        {
            query = query.Where(d => param.LastName.Contains(d.LastName));
        }

        if (param.Specialization?.Any() == true)
        {
            query = query.Where(d => param.Specialization.Contains(d.Specialization));
        }

        if (param.Phone?.Any() == true)
        {
            query = query.Where(d => param.Phone.Contains(d.ContactNumber));
        }

        if (param.Email?.Any() == true)
        {
            query = query.Where(d => param.Email.Contains(d.Email));
        }

        // Sorting
        if (param.Sort?.Any() == true)
        {
            query = param.Sort.Aggregate(
                (IOrderedQueryable<Doctor>)query.OrderBy(GetSortExpression(param.Sort.First(), param.Order)),
                (current, sortField) => current.ThenBy(GetSortExpression(sortField, param.Order))
            );
        }

        return await query.GetPagedResultAsync(param.pageNumber, param.pageSize);
    }

    private Expression<Func<Doctor, object>> GetSortExpression(string field, string order)
    {
        return field.ToLower() switch
        {
            "firstname" => d => d.FirstName,
            "lastname" => d => d.LastName,
            "specialization" => d => d.Specialization,
            "phone" => d => d.ContactNumber,
            "email" => d => d.Email,
            _ => d => d.FirstName
        };
    }
}