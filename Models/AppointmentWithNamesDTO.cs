using System;

namespace HealthcareApi.Models;

public class AppointmentWithNamesDTO
{
    public int? PatientID { get; set; }
    public int? DoctorID { get; set; }
    public int AppointmentID { get; set; }
    public string PatientName { get; set; }
    public string DoctorName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
}