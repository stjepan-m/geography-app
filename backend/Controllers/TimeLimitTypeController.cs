using GeographyGame.Filters;
using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TimeLimitTypeController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<TimeLimitTypeController> _logger;

        public TimeLimitTypeController(GeographyGameContext dbContext, ILogger<TimeLimitTypeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<TimeLimitType>> GetTimeLimitTypes()
        {
            return _dbContext.TimeLimitTypes.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<TimeLimitType> GetTimeLimitTypeById(int id)
        {
            var TimeLimitType = _dbContext.TimeLimitTypes.Find(id);
            if (TimeLimitType == null)
            {
                return NotFound();
            }
            return TimeLimitType;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<TimeLimitType> CreateTimeLimitType([FromBody] TimeLimitType TimeLimitType)
        {
            _dbContext.TimeLimitTypes.Add(TimeLimitType);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetTimeLimitTypeById), new { id = TimeLimitType.Id }, TimeLimitType);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateTimeLimitType(int id, [FromBody] TimeLimitType updatedTimeLimitType)
        {
            var TimeLimitType = _dbContext.TimeLimitTypes.Find(id);
            if (TimeLimitType == null)
            {
                return NotFound();
            }

            TimeLimitType.Name = updatedTimeLimitType.Name;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteTimeLimitType(int id)
        {
            var TimeLimitType = _dbContext.TimeLimitTypes.Find(id);
            if (TimeLimitType == null)
            {
                return NotFound();
            }

            _dbContext.TimeLimitTypes.Remove(TimeLimitType);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("active")]
        public ActionResult<Dictionary<int, IEnumerable<TimeLimitType>>> GetActiveTimeLimitTypesByGameId()
        {
            List<GameTypeTimeLimitType> gameTypeTimeLimitTypes = _dbContext.GameTypeTimeLimitTypes
                .Where(x => x.TimeLimitType.IsActive)
                .Select(x => new GameTypeTimeLimitType
                {
                    Id = x.Id,
                    GameTypeId = x.GameTypeId,
                    TimeLimitType = new TimeLimitType
                    {
                        Id = x.TimeLimitType.Id,
                        Name = x.TimeLimitType.Name,
                        IsActive = x.TimeLimitType.IsActive,
                        IsDefault = x.TimeLimitType.IsDefault,
                        LabelEn = x.TimeLimitType.LabelEn,
                        LabelHr = x.TimeLimitType.LabelHr
                    }
                })
                .ToList();

            return gameTypeTimeLimitTypes.GroupBy(x => x.GameTypeId, x => x.TimeLimitType).ToDictionary(x => x.Key, x => x.AsEnumerable());
        }

        [HttpGet("default")]
        public ActionResult<TimeLimitType> GetDefaultTimeLimitType()
        {
            var TimeLimitTypes = _dbContext.TimeLimitTypes.Where(x => x.IsDefault).ToList();
            if (TimeLimitTypes.Count != 1)
            {
                return NotFound();
            }

            return TimeLimitTypes.First();
        }
    }
}