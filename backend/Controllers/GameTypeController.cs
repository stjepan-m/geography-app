using GeographyGame.Filters;
using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameTypeController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<GameTypeController> _logger;

        public GameTypeController(GeographyGameContext dbContext, ILogger<GameTypeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<GameType>> GetGameTypes()
        {
            return _dbContext.GameTypes.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<GameType> GetGameTypeById(int id)
        {
            var GameType = _dbContext.GameTypes.Find(id);
            if (GameType == null)
            {
                return NotFound();
            }
            return GameType;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<GameType> CreateGameType([FromBody] GameType GameType)
        {
            _dbContext.GameTypes.Add(GameType);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetGameTypeById), new { id = GameType.Id }, GameType);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateGameType(int id, [FromBody] GameType updatedGameType)
        {
            var GameType = _dbContext.GameTypes.Find(id);
            if (GameType == null)
            {
                return NotFound();
            }

            GameType.Name = updatedGameType.Name;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteGameType(int id)
        {
            var GameType = _dbContext.GameTypes.Find(id);
            if (GameType == null)
            {
                return NotFound();
            }

            _dbContext.GameTypes.Remove(GameType);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("active")]
        public ActionResult<IEnumerable<GameType>> GetActiveGameTypes()
        {
            return _dbContext.GameTypes.Where(x => x.IsActive).ToList();
        }
    }
}