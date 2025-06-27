namespace HealthcareApi.Models;

public class PatientQueryParams
{
    public List<int>? PID { get; set; } // Not to be used for querying
    public List<int>? UID { get; set; }
    public List<string>? FirstName { get; set; }
    public List<string>? LastName { get; set; }
    public List<DateOnly>? Dob { get; set; }
    public List<string>? Phone { get; set; }
    public List<string>? Sort { get; set; }
    public string? Order { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}