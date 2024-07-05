using GeographyGame.Models;
using GeographyGame.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeographyGame.Util;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<GameController> _logger;

        public GameController(GeographyGameContext dbContext, ILogger<GameController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<Game>> GetGames()
        {
            return _dbContext.Games.ToList();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpGet("withRelated")]
        public ActionResult<IEnumerable<Game>> GetGamesWithRelated()
        {
            return _dbContext.Games.Include(x => x.TypeNavigation).Include(x => x.RegionNavigation).Include(x => x.ScoringTypeNavigation).Include(x => x.TimeLimitTypeNavigation).ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("myGames/withRelated")]
        public ActionResult<IEnumerable<Game>> GetMyGamesWithRelated()
        {
            if (HttpContext.Items["User"] is User user)
            {
                return _dbContext.Games.Where(x => x.CreatedBy == user.Id).Include(x => x.TypeNavigation).Include(x => x.RegionNavigation).Include(x => x.ScoringTypeNavigation).Include(x => x.TimeLimitTypeNavigation).ToList();
            }
            return Unauthorized();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<Game> GetGameById(string id)
        {
            if (HttpContext.Items["User"] is User user)
            {
                var Game = _dbContext.Games.Find(id);
                if (Game == null || Game.CreatedBy != user.Id && user.UserType != AppConstants.USER_TYPE_ADMINISTRATOR)
                {
                    return NotFound();
                }
                return Game;
            }
            return Unauthorized();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}/info")]
        public ActionResult<Game> GetGameInfoById(string id)
        {
            if (HttpContext.Items["User"] is User user)
            {
                var Game = _dbContext.Games.Where(x => x.Id == id).Include(x => x.TypeNavigation).Include(x => x.RegionNavigation).Include(x => x.ScoringTypeNavigation).Include(x => x.TimeLimitTypeNavigation).FirstOrDefault();
                if (Game == null || Game.CreatedBy != user.Id && user.UserType != AppConstants.USER_TYPE_ADMINISTRATOR)
                {
                    return NotFound();
                }
                return Game;
            }
            return Unauthorized();

        }

        [HttpGet("{id}/withRelated")]
        public ActionResult<Game> GetGameWithRelatedById(string id)
        {
            var Game = _dbContext.Games.Where(x => x.Id == id).Include(x => x.TypeNavigation).Include(x => x.RegionNavigation).Include(x => x.ScoringTypeNavigation).Include(x => x.TimeLimitTypeNavigation).FirstOrDefault();
            if (Game == null)
            {
                return NotFound();
            }
            return Game;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<Game> CreateGame([FromBody] Game Game)
        {
            if (HttpContext.Items["User"] is User user)
            {
                Game.CreatedBy = user.Id;
                _dbContext.Games.Add(Game);
                _dbContext.SaveChanges();
                return CreatedAtAction(nameof(GetGameById), new { id = Game.Id }, Game);
            }
            return Unauthorized();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateGame(string id, [FromBody] Game updatedGame)
        {
            var Game = _dbContext.Games.Find(id);
            if (Game == null)
            {
                return NotFound();
            }

            Game.Name = updatedGame.Name;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteGame(string id)
        {
            var Game = _dbContext.Games.Find(id);
            if (Game == null)
            {
                return NotFound();
            }

            _dbContext.Games.Remove(Game);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPost("withRegionLocations")]
        public ActionResult<Game> CreateGameWithLocationsFromRegion([FromBody] Game Game)
        {

            if (HttpContext.Items["User"] is User user)
            {
                try
                {
                    using var transaction = _dbContext.Database.BeginTransaction();

                    Game.CreatedBy = user.Id;
                    _dbContext.Games.Add(Game);
                    _dbContext.SaveChanges();

                    var locationType = _dbContext.GameTypes.Find(Game.Type).LocationType;
                    var regionLocations = _dbContext.RegionLocations.Where(x => x.Location != null && !x.Location.IsCustom && x.Location.Type == locationType && x.RegionId == Game.Region).ToList();
                    List<GameLocation> locations = regionLocations.ConvertAll<GameLocation>(x => new GameLocation() { GameId = Game.Id, LocationId = x.LocationId });

                    _dbContext.GameLocations.AddRange(locations);
                    _dbContext.SaveChanges();

                    transaction.Commit();

                    return CreatedAtAction(nameof(GetGameById), new { id = Game.Id }, Game);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500);

                }
            }
            return Unauthorized();
            
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPost("withLocations")]
        public ActionResult<Game> CreateGameWithLocations([FromBody] GameWithLocations gameWithLocations)
        {
            if (HttpContext.Items["User"] is User user)
            {
                try
                {
                    using var transaction = _dbContext.Database.BeginTransaction();

                    gameWithLocations.game.CreatedBy = user.Id;
                    _dbContext.Games.Add(gameWithLocations.game);
                    _dbContext.SaveChanges();

                    var locations = _dbContext.Locations.Where(x => gameWithLocations.locationIds.Contains(x.Id)).ToList();
                    List<GameLocation> gameLocations = locations.ConvertAll<GameLocation>(x => new GameLocation() { GameId = gameWithLocations.game.Id, LocationId = x.Id });

                    _dbContext.GameLocations.AddRange(gameLocations);
                    _dbContext.SaveChanges();

                    transaction.Commit();

                    return CreatedAtAction(nameof(GetGameById), new { id = gameWithLocations.game.Id }, gameWithLocations.game);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500);

                }
            }
            return Unauthorized();

        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPut("{id}/addLocationsFromRegion/{regionId}")]
        public IActionResult AddAllLocationsFromRegion(string id, int regionId)
        {
            var Game = _dbContext.Games.Find(id);
            var Region = _dbContext.Regions.Find(regionId);
            if (Game == null || Region == null)
            {
                return NotFound();
            }

            var locationType = _dbContext.GameTypes.Find(Game.Type).LocationType;
            var regionLocations = _dbContext.RegionLocations.Where(x => x.Location != null && !x.Location.IsCustom && x.Location.Type == locationType && x.RegionId == Region.Id).ToList();

            List<GameLocation> locations = regionLocations.ConvertAll<GameLocation>(x => new GameLocation { GameId = Game.Id, LocationId = x.LocationId });
            _dbContext.GameLocations.AddRange(locations);
            _dbContext.SaveChanges();

            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPut("{id}/addLocations")]
        public IActionResult AddLocations(string id, [FromBody] List<int> locationIds)
        {
            var Game = _dbContext.Games.Find(id);
            var locations = _dbContext.Locations.Where(x => locationIds.Contains(x.Id)).ToList();
            if (Game == null || locations.Count == 0)
            {
                return NotFound();
            }

            List<GameLocation> gameLocations = locations.ConvertAll<GameLocation>(x => new GameLocation { GameId = Game.Id, LocationId = x.Id });
            _dbContext.GameLocations.AddRange(gameLocations);
            _dbContext.SaveChanges();

            return NoContent();
        }

        public class GameWithLocations
        {
            public Game? game { get; set; }
            public IEnumerable<int>? locationIds { get; set; }
        }
    }
}