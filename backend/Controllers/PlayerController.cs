using GeographyGame.Filters;
using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(GeographyGameContext dbContext, ILogger<PlayerController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<Player>> GetPlayers()
        {
            return _dbContext.Players.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<Player> GetPlayerById(int id)
        {
            var Player = _dbContext.Players.Find(id);
            if (Player == null)
            {
                return NotFound();
            }
            return Player;
        }

        [HttpPost]
        public ActionResult<Player> CreatePlayer([FromBody] Player Player)
        {
            _dbContext.Players.Add(Player);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetPlayerById), new { id = Player.Id }, Player);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdatePlayer(int id, [FromBody] Player updatedPlayer)
        {
            var Player = _dbContext.Players.Find(id);
            if (Player == null)
            {
                return NotFound();
            }

            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeletePlayer(int id)
        {
            var Player = _dbContext.Players.Find(id);
            if (Player == null)
            {
                return NotFound();
            }

            _dbContext.Players.Remove(Player);
            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}
