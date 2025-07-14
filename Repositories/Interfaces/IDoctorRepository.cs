using HealthcareApi.Models;

namespace HealthcareApi.Repositories;

public interface IDoctorRepository
{
    IQueryable<Doctor> GetBaseQuery();
    Task<Doctor> GetByIdAsync(int id);
    Task<Doctor> AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task<List<CurrentAppointmentsDTO>> GetTodayAppointmentsAsync(int? doctorId, string? statusFilter);
}