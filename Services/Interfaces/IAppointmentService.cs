using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> AddAppointment(Appointment appointment);
        Task<bool> SoftDeleteAppointment(int id);
        Task UpdateAppointment(int id, JsonPatchDocument patch);
        Task<Appointment?> GetAppointmentById(int id);
        Task<PagedResult<AppointmentWithNamesDTO>> SearchAppointments(AppointmentQueryParams queryParams);
    }
}
