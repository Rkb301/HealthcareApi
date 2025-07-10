using HealthcareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AssignmentDbContext _context;
    public AppointmentRepository(AssignmentDbContext ctx) => _context = ctx;

    public IQueryable<Appointment> GetBaseQuery() =>
        _context.Appointments.Where(a => a.isActive);

    public async Task<Appointment?> GetByIdAsync(int id) =>
        await _context.Appointments
            .Where(a => a.isActive && a.AppointmentID == id)
            .FirstOrDefaultAsync();

    public async Task<Appointment> AddAsync(Appointment a)
    {
        _context.Appointments.Add(a);
        await _context.SaveChangesAsync();
        return a;
    }

    public async Task UpdateAsync(Appointment a)
    {
        _context.Entry(a).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
