using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApi.Models;

public partial class Appointment : IValidatableObject
{
    public int AppointmentID { get; set; }

    [Required(ErrorMessage = "PatientID is required")]
    public int PatientID { get; set; }

    [Required(ErrorMessage = "DoctorID is required")]
    public int DoctorID { get; set; }

    [Required(ErrorMessage = "Appointment date is required")]
    [DataType(DataType.DateTime)]
    [FutureDate(ErrorMessage = "Appointment date must be in the future")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Reason is required")]
    [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
    public string? Reason { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    [RegularExpression("^(Scheduled|Completed|Cancelled)$", 
        ErrorMessage = "Status must be Scheduled, Completed, or Cancelled")]
    public string? Status { get; set; } = "Scheduled";

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    public bool isActive { get; set; } = true;

    [ForeignKey("DoctorID")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("PatientID")]
    public virtual Patient Patient { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AppointmentDate < DateTime.UtcNow.AddMinutes(30))
        {
            yield return new ValidationResult(
                "Appointments must be scheduled at least 30 minutes in advance",
                new[] { nameof(AppointmentDate) });
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value is DateTime date && date > DateTime.UtcNow;
    }
}
