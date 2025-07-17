using System.ComponentModel.DataAnnotations;

namespace HealthcareApi.Models;

public class UserQueryParams
{
    public List<int>? UID { get; set; } // Not to be used for querying
    public List<string>? Username { get; set; }
    public List<string>? Role { get; set; }
    // [EmailAddress(ErrorMessage = "Invalid email format")]
    public List<string>? Email { get; set; }
    public List<DateTime>? CreatedAt { get; set; }
    public List<DateTime>? ModifiedAt { get; set; }
    public List<bool>? isActive { get; set; }
    public List<string>? Sort { get; set; }
    public string? Order { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
