using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareApi.Models;

public partial class User
{
    public int UserID { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, 
        ErrorMessage = "Username must be 3-50 characters")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password hash is required")]
    [StringLength(255, MinimumLength = 6, 
        ErrorMessage = "Password must be at least 6 characters")]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Role is required")]
    [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
    [RegularExpression("^(Admin|Doctor|Patient)$", 
        ErrorMessage = "Role must be Admin, Doctor, or Patient")]
    public string Role { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DataType(DataType.DateTime)]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    
    public bool isActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
