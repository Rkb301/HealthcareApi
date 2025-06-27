using Azure;
using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public interface IAppointmentService
{
    Task<List<Appointment>> GetAllAppointments();
    Task<Appointment> GetAppointmentById(int id);
    Task<Appointment> AddAppointment(Appointment appointment);
    Task UpdateAppointment(int id, JsonPatchDocument<Appointment> patchDoc);
    Task<bool> SoftDeleteAppointment(int id);
    Task<PagedResult<Appointment>> SearchAppointments(AppointmentQueryParams param);
}