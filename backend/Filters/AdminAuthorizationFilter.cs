using GeographyGame.Models;
using GeographyGame.Util;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;

namespace GeographyGame.Filters
{
    public class AdminAuthorizationFilter : BaseAuthorizationFilter
    {
        public AdminAuthorizationFilter(GeographyGameContext dbContext) : base(dbContext)
        {
        }

        protected override async Task<Session> GetSessionAsync(string token)
        {
            return await _dbContext.Sessions.Where(s => s.User.UserType == AppConstants.USER_TYPE_ADMINISTRATOR).Include(s => s.User).SingleOrDefaultAsync(s => s.SessionToken == token);
        }
    }
}
