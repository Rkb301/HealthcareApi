using System;
using System.Linq.Expressions;
using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repo;
    private readonly LuceneDoctorIndexService _lucene;

    public DoctorService(
        IDoctorRepository repo,
        LuceneDoctorIndexService lucene)
    {
        _repo = repo;
        _lucene = lucene;
    }

    public async Task<Doctor> AddDoctor(Doctor d)
    {
        d.CreatedAt = DateTime.UtcNow;
        d.ModifiedAt = DateTime.UtcNow;
        var ret = await _repo.AddAsync(d);
        _lucene.IndexDoctor(ret);
        return ret;
    }

    public async Task<bool> SoftDeleteDoctor(int id)
    {
        var d = await _repo.GetByIdAsync(id);
        if (d == null) return false;
        d.isActive = false;
        d.ModifiedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(d);
        _lucene.IndexDoctor(d);
        return true;
    }

    public async Task UpdateDoctor(int id, JsonPatchDocument patch)
    {
        var d = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
        patch.ApplyTo(d);
        d.ModifiedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(d);
        _lucene.IndexDoctor(d);
    }

    public async Task<Doctor?> GetDoctorById(int id) =>
        await _repo.GetByIdAsync(id);

    public async Task<PagedResult<Doctor>> SearchDoctorsLucene(DoctorQueryParams qp)
    {
        if (string.IsNullOrWhiteSpace(qp.Query))
        {
            return await _repo.GetBaseQuery()
                .GetPagedResultAsync(qp.pageNumber, qp.pageSize);
        }
        return _lucene.Search(qp.Query, qp.pageNumber, qp.pageSize, qp.Sort?.FirstOrDefault(), qp.Order);
    }

    public async Task<List<CurrentAppointmentsDTO>> GetPresentAppointments(int? id, string? status)
    {
        return await _repo.GetTodayAppointmentsAsync(id, status);
    }
    
    public async Task<PagedResult<Doctor>> SearchDoctors(DoctorQueryParams qp)
    {
        var query = _repo.GetBaseQuery();

        if (qp.UID?.Any() == true)
        {
            query = query.Where(d => qp.UID.Contains(d.UserID));
        }

        if (qp.Email?.Any() == true)
        {
            query = query.Where(d => qp.Email.Contains(d.Email));
        }

        if (qp.Specialization?.Any() == true)
        {
            query = query.Where(d => qp.Specialization == d.Specialization);
        }

        if (qp.Sort?.Any() == true)
        {
            query = qp.Sort.Aggregate(
                (IOrderedQueryable<Doctor>)query.OrderBy(GetSortExpression(qp.Sort.First(), qp.Order)),
                (current, sortField) => current.ThenBy(GetSortExpression(sortField, qp.Order))
            );
        }

        return await query.GetPagedResultAsync(qp.pageNumber, qp.pageSize);
    }
    
    private Expression<Func<Doctor, object>> GetSortExpression(string field, string order)
    {
        return field.ToLower() switch
        {
            "userid" => d => d.UserID,
            "email" => d => d.Email,
            _ => d => d.DoctorID
        };
    }
}
