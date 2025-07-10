using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AssignmentDbContext _context;
    public DoctorRepository(AssignmentDbContext ctx) => _context = ctx;

    public IQueryable<Doctor> GetBaseQuery() =>
        _context.Doctors.Where(d => d.isActive);

    public async Task<Doctor?> GetByIdAsync(int id) =>
        await _context.Doctors
            .Where(d => d.isActive && d.DoctorID == id)
            .FirstOrDefaultAsync();

    public async Task<Doctor> AddAsync(Doctor d)
    {
        _context.Doctors.Add(d);
        await _context.SaveChangesAsync();
        return d;
    }

    public async Task UpdateAsync(Doctor d)
    {
        _context.Entry(d).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
