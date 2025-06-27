using HealthcareApi.Models;

namespace HealthcareApi.Repositories;

public interface IUserRepository
{
    IQueryable<User> GetBaseQuery();
    Task<User> GetByIdAsync(int id);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}