// MedicalRecord.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApi.Models
{
    public partial class MedicalRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RecordID { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        [Required]
        [StringLength(255)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Treatment { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Medications { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
