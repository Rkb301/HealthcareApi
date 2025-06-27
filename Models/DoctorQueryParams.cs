using System.ComponentModel.DataAnnotations;

namespace HealthcareApi.Models;

public class DoctorQueryParams
{
    public List<int>? DID { get; set; }
    public List<int>? UID { get; set; }
    public List<string>? FirstName { get; set; }
    public List<string>? LastName { get; set; }
    public List<string>? Specialization { get; set; }
    [Phone]
    public List<string>? Phone { get; set; }
    [EmailAddress]
    public List<string>? Email { get; set; }
    public List<DateTime> CreatedAt { get; set; }
    public List<DateTime> ModifiedAt { get; set; }
    public List<bool>? isActive { get; set; }
    public List<string>? Sort { get; set; }
    public string? Order { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}