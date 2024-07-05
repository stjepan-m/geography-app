using GeographyGame.Filters;
using GeographyGame.Models;
using GeographyGame.Util;
using GeographyGame.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerGameController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<PlayerGameController> _logger;

        public PlayerGameController(GeographyGameContext dbContext, ILogger<PlayerGameController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<PlayerGame>> GetPlayerGames([FromQuery] string? gameId)
        {
            return _dbContext.PlayerGames
                .Where(x=> gameId == null || x.GameId == gameId)
                .Include(x => x.Game)
                .Include(x => x.Player)
                .Include(x => x.Game.ScoringTypeNavigation)
                .ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<PlayerGame> GetPlayerGameById(int id)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }
            return PlayerGame;
        }

        [ServiceFilter(typeof(OptionalAuthorizationFilter))]
        [HttpPost]
        public ActionResult<PlayerGame> CreatePlayerGame([FromBody] PlayerGame PlayerGame)
        {
            if (HttpContext.Items["User"] is User user)
            {
                PlayerGame.PlayerId = _dbContext.Players.Where(x => x.UserId == user.Id).FirstOrDefault().Id;
            }
            _dbContext.PlayerGames.Add(PlayerGame);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetPlayerGameById), new { id = PlayerGame.Id }, PlayerGame);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdatePlayerGame(int id, [FromBody] PlayerGame updatedPlayerGame)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeletePlayerGame(int id)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            _dbContext.PlayerGames.Remove(PlayerGame);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(OptionalAuthorizationFilter))]
        [HttpGet("suspended")]
        public ActionResult<IEnumerable<PlayerGame>> GetSuspended([FromQuery] int? playerId, [FromQuery] string? gameId)
        {
            try
            {
                if (HttpContext.Items["User"] is User user)
                {
                    playerId = _dbContext.Players.Where(x => x.UserId == user.Id).FirstOrDefault().Id;
                }
                
                var playerGames = _dbContext.PlayerGames.Where(x => x.Status == AppConstants.GAME_STATUS_SUSPENDED && (playerId == null || x.PlayerId == playerId) && (gameId == null || x.GameId == gameId)).ToList();

                return playerGames;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500);
            }
        }

        [HttpPost("{id}/suspend")]
        public IActionResult SuspendGame(int id, [FromQuery] int timeLeft)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            if(PlayerGame.Status == AppConstants.GAME_STATUS_IN_PROGRESS)
            {
                PlayerGame.Status = AppConstants.GAME_STATUS_SUSPENDED;
                PlayerGame.TimeLeft = timeLeft;
                _dbContext.SaveChanges();
            }
            
            return NoContent();
        }

        [HttpPut("{id}/start")]
        public IActionResult StartGame(int id)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            PlayerGame.Status = AppConstants.GAME_STATUS_IN_PROGRESS;

            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        public IActionResult CancelGame(int id)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            PlayerGame.Status = AppConstants.GAME_STATUS_CANCELLED;

            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id}/complete")]
        public IActionResult CompleteGame(int id)
        {
            var PlayerGame = _dbContext.PlayerGames.Find(id);
            if (PlayerGame == null)
            {
                return NotFound();
            }

            PlayerGame.Status = AppConstants.GAME_STATUS_COMPLETED;
            PlayerGame.TimeLeft = null;

            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}
