using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services
{
    public interface IDoctorService
    {
        Task<Doctor> AddDoctor(Doctor doctor);
        Task<bool> SoftDeleteDoctor(int id);
        Task UpdateDoctor(int id, JsonPatchDocument patch);
        Task<Doctor?> GetDoctorById(int id);
        Task<PagedResult<Doctor>> SearchDoctorsLucene(DoctorQueryParams queryParams);
        Task<PagedResult<Doctor>> SearchDoctors(DoctorQueryParams queryParams);
        public Task<List<CurrentAppointmentsDTO>> GetPresentAppointments(int? id, string? status);
    }
}
