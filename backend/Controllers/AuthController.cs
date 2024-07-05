using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using GeographyGame.Util;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<CityController> _logger;

        public AuthController(GeographyGameContext dbContext, ILogger<CityController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("google/login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { "443373306977-g5cclsjtle2sdns1ne9osdfrde64cs7r.apps.googleusercontent.com" }
                });

                // Check if user exists
                var user = _dbContext.Users.FirstOrDefault(u => u.GoogleId == payload.Subject);
                if (user == null)
                {
                    // Create new user
                    user = new User
                    {
                        GoogleId = payload.Subject,
                        Email = payload.Email,
                        Name = payload.Name,
                        UserType = AppConstants.USER_TYPE_TEACHER
                    };
                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();

                    var player = new Player
                    {
                        UserId = user.Id,
                        Nickname = user.Name
                    };
                    _dbContext.Players.Add(player);
                    await _dbContext.SaveChangesAsync();
                }

                // Create session
                var sessionToken = Guid.NewGuid().ToString();
                var session = new Session
                {
                    UserId = user.Id,
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // 7 days session validity
                };
                _dbContext.Sessions.Add(session);
                await _dbContext.SaveChangesAsync();

                transaction.Commit();
                return Ok(new { sessionToken, user });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized("Invalid ID token");
            }
        }
    }

    public class GoogleLoginRequest
    {
        public string Token { get; set; } = null!;
    }
}