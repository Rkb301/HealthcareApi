using System;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Models
{
    [Keyless]
    public class UpcomingAppointmentDTO
    {
        public DateOnly Date                  { get; set; } // Date part of appointmentDate
        public TimeOnly Time                  { get; set; } // Time part of appointmentDate
        public string? Doctor                 { get; set; } // Doctor full name
        public string? Reason                 { get; set; }
        public string? Status                 { get; set; }
    }
}
