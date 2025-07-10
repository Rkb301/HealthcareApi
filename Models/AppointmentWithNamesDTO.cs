namespace HealthcareApi.Models;

public class AppointmentWithNamesDTO
{
    public int AppointmentID   { get; set; }
    public string PatientName  { get; set; } = null!;
    public string DoctorName   { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public string? Reason      { get; set; }
    public string? Status      { get; set; }
    public string? Notes       { get; set; }
}
