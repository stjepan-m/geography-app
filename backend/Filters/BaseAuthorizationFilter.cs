using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GeographyGame.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace GeographyGame.Filters;

public class BaseAuthorizationFilter : IAsyncAuthorizationFilter
{
    protected readonly GeographyGameContext _dbContext;

    public BaseAuthorizationFilter(GeographyGameContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        if (httpContext.Request.Headers.TryGetValue("Authorization", out StringValues tokenValues))
        {
            var token = tokenValues.FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

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
        else
        {
            context.Result = new UnauthorizedResult();
        }
    }

    protected virtual async Task<Session> GetSessionAsync(string token)
    {
        return await _dbContext.Sessions.Include(s => s.User).SingleOrDefaultAsync(s => s.SessionToken == token);
    }
}