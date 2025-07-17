using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public interface IPatientService
{
    Task<List<Patient>> GetAllPatients();
    Task<Patient> GetPatientById(int id);
    Task<Patient> AddPatient(Patient patient);
    Task UpdatePatient(int id, JsonPatchDocument<Patient> patchDoc);
    Task<bool> SoftDeletePatient(int id);
    Task<PagedResult<Patient>> SearchPatients(PatientQueryParams param);
    public Task<List<UpcomingAppointmentDTO>> GetUpcomingAppointmentsAsync(int? id, string? status);
}

public class NotFoundException : Exception { }
