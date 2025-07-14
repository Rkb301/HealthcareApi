using System;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Models
{
    [Keyless]
    public class CurrentAppointmentsDTO
    {
        // Appointment identifiers
        public int AppointmentID              { get; set; }
        public int PatientID                  { get; set; }
        public int DoctorID                   { get; set; }

        // Appointment details
        public DateTime Date                  { get; set; }
        public string? Reason                 { get; set; }
        public string? Status                 { get; set; }
        public string? Notes                  { get; set; }

        // Patient details
        public string PatientName             { get; set; } = null!;
        public DateOnly DOB                   { get; set; }
        public string Gender                  { get; set; } = null!;
        public string Contact                 { get; set; } = null!;
        public string MedicalHistory          { get; set; } = null!;
        public string Allergies               { get; set; } = null!;
        public string CurrentMedications      { get; set; } = null!;
    }
}
