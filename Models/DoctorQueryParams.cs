namespace HealthcareApi.Models;

public class DoctorQueryParams
{
    public List<int>? UID { get; set; }
    public List<string>? Email { get; set; }
    public string? Specialization { get; set; }
    public string? Query { get; set; }
    public List<string>? Sort { get; set; }
    public string? Order { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
