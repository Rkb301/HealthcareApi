using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Attributes;

namespace HealthcareApi.Models;

public partial class User
{
    public int UserID { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
