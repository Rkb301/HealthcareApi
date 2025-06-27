using HealthcareApi.Models;

namespace HealthcareApi.Repositories;

public interface IAppointmentRepository
{
    IQueryable<Appointment> GetBaseQuery();
    Task<Appointment> GetByIdAsync(int id);
    Task<Appointment> AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
}