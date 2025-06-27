using Azure;
using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public interface IDoctorService
{
    Task<List<Doctor>> GetAllDoctors();
    Task<Doctor> GetDoctorById(int id);
    Task<Doctor> AddDoctor(Doctor doctor);
    Task UpdateDoctor(int id, JsonPatchDocument<Doctor> patchDoc);
    Task<bool> SoftDeleteDoctor(int id);
    Task<PagedResult<Doctor>> SearchDoctors(DoctorQueryParams param);
}
