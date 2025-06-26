public class SessionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionHandlingMiddleware> _logger;
    private readonly IHttpContextAccessor _contextAccessor;

    public SessionHandlingMiddleware(
        RequestDelegate next,
        ILogger<SessionHandlingMiddleware> logger,
        IHttpContextAccessor context)
    {
        _next = next;
        _logger = logger;
        _contextAccessor = context;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            //
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}