using HealthcareApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace HealthcareApi.Services;

public interface IUserService
{
    Task<List<User>> GetAllUsers();
    Task<User> GetUserById(int id);
    Task<User> AddUser(User user);
    Task UpdateUser(int id, JsonPatchDocument<User> patchDoc);
    Task<bool> SoftDeleteUser(int id);
    Task<PagedResult<User>> SearchUsers(UserQueryParams param);
}