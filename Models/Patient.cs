using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace HealthcareApi.Models
{
    public partial class Patient : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PatientID { get; set; }

        [ForeignKey("User")]
        public int? UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [DataType(DataType.Date)]
        [AllowNull]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [Phone]
        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        // PRESERVED: Additional medical fields
        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [StringLength(500)]
        public string? CurrentMedications { get; set; }

        // PRESERVED: Emergency contact fields (added to maintain compatibility)
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;

        // PRESERVED: Navigation properties with JsonIgnore
        [JsonIgnore]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [JsonIgnore]
        public virtual User? User { get; set; } = null!;

        [JsonIgnore]                                     
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } 
            = new List<MedicalRecord>();

        [JsonIgnore]
        public virtual ICollection<Prescription> Prescriptions { get; set; } 
            = new List<Prescription>();
        
        [JsonIgnore]
        public virtual ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();


        // PRESERVED: Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth.HasValue && DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Date of birth cannot be in the future.",
                    new[] { nameof(DateOfBirth) });
            }
        }
    }
}
