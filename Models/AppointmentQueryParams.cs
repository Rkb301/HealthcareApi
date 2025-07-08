namespace HealthcareApi.Models;

public class AppointmentQueryParams
{
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public List<DateTime>? AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public List<string>? Status { get; set; }
    public string? Notes { get; set; }
    public List<string>? Sort { get; set; }
    public string? Order { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}