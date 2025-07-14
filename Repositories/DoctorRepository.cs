using HealthcareApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Repositories
{
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

        public async Task<List<CurrentAppointmentsDTO>> GetTodayAppointmentsAsync(int? doctorId, string? statusFilter)
        {
            var doctorParam = new SqlParameter("@DoctorID",
                doctorId.HasValue ? doctorId.Value : (object)DBNull.Value);

            var statusParam = new SqlParameter("@StatusFilter",
                statusFilter != null ? statusFilter : (object)DBNull.Value);

            var appointments = await _context
                .Set<CurrentAppointmentsDTO>()
                .FromSqlInterpolated($@"
                    EXEC dbo.GetTodayAppointDoctor 
                        @DoctorID = {doctorParam}, 
                        @StatusFilter = {statusParam}
                ")
                .ToListAsync();

            return appointments;
        }
    }
}
