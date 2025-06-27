using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AssignmentDbContext _context;

    public DoctorRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public IQueryable<Doctor> GetBaseQuery() => _context.Doctors.AsQueryable();

    public async Task<Doctor> GetByIdAsync(int id)
    {
        return await _context.Doctors.FindAsync(id);
    }

    public async Task<Doctor> AddAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _context.Entry(doctor).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}