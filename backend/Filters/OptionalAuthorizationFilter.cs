// Filters/CustomAuthorizationFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GeographyGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace GeographyGame.Filters;

public class OptionalAuthorizationFilter : IAsyncAuthorizationFilter
{
    protected readonly GeographyGameContext _dbContext;

    public OptionalAuthorizationFilter(GeographyGameContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        if (httpContext.Request.Headers.TryGetValue("Authorization", out StringValues tokenValues))
        {
            var token = tokenValues.FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                var session = await GetSessionAsync(token);
                if (session == null || session.ExpiresAt <= DateTime.UtcNow)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    httpContext.Items["User"] = session.User;
                }
            }
        }
    }

    protected virtual async Task<Session> GetSessionAsync(string token)
    {
        return await _dbContext.Sessions.Include(s => s.User).SingleOrDefaultAsync(s => s.SessionToken == token);
    }
}