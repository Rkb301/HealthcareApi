using HealthcareApi.Models;
using System.Linq.Expressions;

namespace HealthcareApi.Repositories;

public interface IPatientRepository
{
    IQueryable<Patient> GetBaseQuery();
    Task<Patient> GetByIdAsync(int id);
    Task<Patient> AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
}
