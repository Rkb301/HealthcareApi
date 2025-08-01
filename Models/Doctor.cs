using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace HealthcareApi.Models
{
    public partial class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "UserID is required")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization cannot exceed 100 characters")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Schedule is required")]
        [StringLength(255, ErrorMessage = "Schedule cannot exceed 255 characters")]
        public string? Schedule { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;

        // PRESERVED: Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [JsonIgnore]
        [ForeignKey("UserID")]
        public virtual User? User { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<MedicalRecord>? MedicalRecords { get; set; }
            = new List<MedicalRecord>();

        [JsonIgnore]
        public virtual ICollection<Prescription>? Prescriptions { get; set; }
            = new List<Prescription>();
        
        [JsonIgnore]
        public virtual ICollection<LabResult>? LabResults { get; set; } = new List<LabResult>();
    }
}
