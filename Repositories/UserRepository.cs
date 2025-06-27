using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AssignmentDbContext _context;

    public UserRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public IQueryable<User> GetBaseQuery() => _context.Users.AsQueryable();

    public async Task<User> GetByIdAsync(int id)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return await _context.Users.FindAsync(id);
#pragma warning restore CS8603 // Possible null reference return.
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}