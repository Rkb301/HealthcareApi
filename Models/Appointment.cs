using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Attributes;

namespace HealthcareApi.Models;

public partial class Appointment
{
    public int AppointmentID { get; set; }

    [Required]
    public int PatientID { get; set; }

    [Required]
    public int DoctorID { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
