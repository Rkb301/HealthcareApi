using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Attributes;

namespace HealthcareApi.Models;

public partial class Doctor
{
    public int DoctorID { get; set; }

    [Required]
    public int UserID { get; set; }

    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string? LastName { get; set; }

    [StringLength(100)]
    public string? Specialization { get; set; }

    [Phone]
    [StringLength(20)]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Schedule { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}
