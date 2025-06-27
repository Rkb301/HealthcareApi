using System.Linq.Expressions;
using Azure;
using HealthcareApi.Extensions;
using HealthcareApi.Models;
using HealthcareApi.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _repository.GetBaseQuery().ToListAsync();
    }

    public async Task<User> GetUserById(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<User> AddUser(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.ModifiedAt = DateTime.UtcNow;
        return await _repository.AddAsync(user);
    }

    public async Task UpdateUser(int id, JsonPatchDocument<User> patchDoc)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) throw new NotFoundException();

        patchDoc.ApplyTo(user);
        user.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(user);
    }

    public async Task<bool> SoftDeleteUser(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return false;

        user.isActive = false;
        user.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<PagedResult<User>> SearchUsers(UserQueryParams param)
    {
        var query = _repository.GetBaseQuery();

        if (param.Username?.Any() == true)
        {
            query = query.Where(u => param.Username.Contains(u.Username));
        }

        if (param.Role?.Any() == true)
        {
            query = query.Where(u => param.Role.Contains(u.Role));
        }

        if (param.Email?.Any() == true)
        {
            query = query.Where(u => param.Email.Contains(u.Email));
        }

        if (param.CreatedAt?.Any() == true)
        {
            query = query.Where(u => param.CreatedAt.Contains(u.CreatedAt));
        }

        if (param.ModifiedAt?.Any() == true)
        {
            query = query.Where(u => param.ModifiedAt.Contains(u.ModifiedAt));
        }

        if (param.isActive?.Any() == true)
        {
            query = query.Where(u => param.isActive.Contains(u.isActive));
        }


        // Sorting
        if (param.Sort?.Any() == true)
        {
            query = param.Sort.Aggregate(
                (IOrderedQueryable<User>)query.OrderBy(GetSortExpression(param.Sort.First(), param.Order)),
                (current, sortField) => current.ThenBy(GetSortExpression(sortField, param.Order))
            );
        }

        return await query.GetPagedResultAsync(param.pageNumber, param.pageSize);
    }

    private Expression<Func<User, object>> GetSortExpression(string field, string order)
    {
        return field.ToLower() switch
        {
            "userid" => u => u.UserID,
            "username" => u => u.Username,
            "role" => u => u.Role,
            "email" => u => u.Email,
            "createdat" => u => u.CreatedAt,
            "modifiedat" => u => u.ModifiedAt,
            "isactive" => u => u.isActive,
            _ => u => u.UserID
        };
    }
}