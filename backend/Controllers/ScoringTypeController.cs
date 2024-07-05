using GeographyGame.Filters;
using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScoringTypeController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<ScoringTypeController> _logger;

        public ScoringTypeController(GeographyGameContext dbContext, ILogger<ScoringTypeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<ScoringType>> GetScoringTypes()
        {
            return _dbContext.ScoringTypes.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<ScoringType> GetScoringTypeById(int id)
        {
            var ScoringType = _dbContext.ScoringTypes.Find(id);
            if (ScoringType == null)
            {
                return NotFound();
            }
            return ScoringType;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<ScoringType> CreateScoringType([FromBody] ScoringType ScoringType)
        {
            _dbContext.ScoringTypes.Add(ScoringType);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetScoringTypeById), new { id = ScoringType.Id }, ScoringType);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateScoringType(int id, [FromBody] ScoringType updatedScoringType)
        {
            var ScoringType = _dbContext.ScoringTypes.Find(id);
            if (ScoringType == null)
            {
                return NotFound();
            }

            ScoringType.Name = updatedScoringType.Name;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteScoringType(int id)
        {
            var ScoringType = _dbContext.ScoringTypes.Find(id);
            if (ScoringType == null)
            {
                return NotFound();
            }

            _dbContext.ScoringTypes.Remove(ScoringType);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("active")]
        public ActionResult<Dictionary<int, IEnumerable<ScoringType>>> GetActiveScoringTypesByGameId()
        {
            List<GameTypeScoringType> gameTypeScoringTypes = _dbContext.GameTypeScoringTypes
                .Where(x => x.ScoringType.IsActive)
                .Select(x => new GameTypeScoringType
                {
                    Id = x.Id,
                    GameTypeId = x.GameTypeId,
                    ScoringType = new ScoringType
                    {
                        Id = x.ScoringType.Id,
                        Name = x.ScoringType.Name,
                        IsActive = x.ScoringType.IsActive,
                        Formula = x.ScoringType.Formula,
                        MaxScore = x.ScoringType.MaxScore,
                        LabelEn = x.ScoringType.LabelEn,
                        LabelHr = x.ScoringType.LabelHr
                    }
                })
                .ToList();

            return gameTypeScoringTypes.GroupBy(x => x.GameTypeId, x => x.ScoringType).ToDictionary(x => x.Key, x => x.AsEnumerable());
        }
    }
}