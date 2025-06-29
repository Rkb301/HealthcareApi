﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthcareApi.Models;

public partial class Patient : IValidatableObject
{
    public int PatientID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [Phone]
    [StringLength(20)]
    public string? ContactNumber { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(1000)]
    public string? MedicalHistory { get; set; }

    [StringLength(500)]
    public string? Allergies { get; set; }

    [StringLength(500)]
    public string? CurrentMedications { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    public bool isActive { get; set; } = true;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual User User { get; set; } = null!;

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
