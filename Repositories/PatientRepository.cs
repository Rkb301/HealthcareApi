using HealthcareApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AssignmentDbContext _context;

    public PatientRepository(AssignmentDbContext context)
    {
        _context = context;
    }

    public IQueryable<Patient> GetBaseQuery() => _context.Patients.Where(p => p.isActive == true).AsQueryable();

    public async Task<Patient> GetByIdAsync(int id)
    {
        return await _context.Patients
        .Where(p => p.isActive)
        .FirstOrDefaultAsync(p => p.PatientID == id);
    }

    public async Task<Patient> AddAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Entry(patient).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<List<UpcomingAppointmentDTO>> GetUpcomingAppointmentsAsync(int? patientID, string? statusFilter)
    {
        var patientParam = new SqlParameter("@PatientID",
                patientID.HasValue ? patientID.Value : (object)DBNull.Value);

        var statusParam = new SqlParameter("@StatusFilter",
                statusFilter != null ? statusFilter : (object)DBNull.Value);

        var appointments = await _context
                .Set<UpcomingAppointmentDTO>()
                .FromSqlInterpolated($@"
                    EXEC dbo.usp_PatientUpcomingAppointmentsGet 
                        @PatientID = {patientParam}, 
                        @StatusFilter = {statusParam}
                ")
                .ToListAsync();

        return appointments;
    }
}
