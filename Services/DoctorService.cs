using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public class DoctorService: IDoctorService
{
    private readonly IDoctorRepository _repo;
    private readonly LuceneDoctorIndexService _lucene;

    public DoctorService(
        IDoctorRepository repo,
        LuceneDoctorIndexService lucene)
    {
        _repo   = repo;
        _lucene = lucene;
    }

    public async Task<Doctor> AddDoctor(Doctor d)
    {
        d.CreatedAt  = DateTime.UtcNow;
        d.ModifiedAt = DateTime.UtcNow;
        var ret = await _repo.AddAsync(d);
        _lucene.IndexDoctor(ret);
        return ret;
    }

    public async Task<bool> SoftDeleteDoctor(int id)
    {
        var d = await _repo.GetByIdAsync(id);
        if (d == null) return false;
        d.isActive    = false;
        d.ModifiedAt  = DateTime.UtcNow;
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

    public async Task<PagedResult<Doctor>> SearchDoctors(DoctorQueryParams qp)
    {
        if (string.IsNullOrWhiteSpace(qp.Query))
        {
            return await _repo.GetBaseQuery()
                .GetPagedResultAsync(qp.pageNumber, qp.pageSize);
        }
        return _lucene.Search(qp.Query, qp.pageNumber, qp.pageSize, qp.Sort?.FirstOrDefault(), qp.Order);
    }
}
