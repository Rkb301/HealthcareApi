using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApi.Models
{
    public partial class LabResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LabResultID { get; set; }

        [Required(ErrorMessage = "PatientID is required")]
        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "DoctorID is required")]
        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Test name is required")]
        [StringLength(255, ErrorMessage = "Test name cannot exceed 255 characters")]
        public string TestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Result is required")]
        [StringLength(255, ErrorMessage = "Result cannot exceed 255 characters")]
        public string Result { get; set; } = string.Empty;

        [Required(ErrorMessage = "Normal range is required")]
        [StringLength(100, ErrorMessage = "Normal range cannot exceed 100 characters")]
        public string NormalRange { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [RegularExpression("^(Normal|High|Low|Abnormal|Critical)$", ErrorMessage = "Status must be Normal, High, Low, Abnormal, or Critical")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Test date is required")]
        [DataType(DataType.DateTime)]
        public DateTime TestDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorID")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
