using Microsoft.EntityFrameworkCore;

namespace HealthcareApi.Extensions;

public static class QueryExtensions
{
    public static async Task<PagedResult<T>> GetPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        int totalCount = await query.CountAsync();
        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
