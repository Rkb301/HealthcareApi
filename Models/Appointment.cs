using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace HealthcareApi.Models
{
    public partial class Appointment : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Appointment date must be in the future")]
        public DateTime AppointmentDate { get; set; }

        [StringLength(255, ErrorMessage = "Reason cannot exceed 255 characters")]
        public string? Reason { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Scheduled|Completed|Cancelled)$", ErrorMessage = "Status must be Scheduled, Completed, or Cancelled")]
        public string? Status { get; set; } = "Scheduled";

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;

        [JsonIgnore]
        [ForeignKey("DoctorID")]
        public virtual Doctor? Doctor { get; set; } = null!;

        [JsonIgnore]
        [ForeignKey("PatientID")]
        public virtual Patient? Patient { get; set; } = null!;

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
        public override bool IsValid(object? value)
        {
            return value is DateTime date && date > DateTime.UtcNow;
        }
    }
}
