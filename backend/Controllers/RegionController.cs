using GeographyGame.Filters;
using GeographyGame.Models;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegionController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<RegionController> _logger;

        public RegionController(GeographyGameContext dbContext, ILogger<RegionController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<Region>> GetRegions()
        {
            return _dbContext.Regions.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<Region> GetRegionById(int id)
        {
            var Region = _dbContext.Regions.Find(id);
            if (Region == null)
            {
                return NotFound();
            }
            return Region;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<Region> CreateRegion([FromBody] Region Region)
        {
            _dbContext.Regions.Add(Region);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetRegionById), new { id = Region.Id }, Region);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateRegion(int id, [FromBody] Region updatedRegion)
        {
            var Region = _dbContext.Regions.Find(id);
            if (Region == null)
            {
                return NotFound();
            }

            Region.Name = updatedRegion.Name;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteRegion(int id)
        {
            var Region = _dbContext.Regions.Find(id);
            if (Region == null)
            {
                return NotFound();
            }

            _dbContext.Regions.Remove(Region);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("active")]
        public ActionResult<IEnumerable<Region>> GetActiveRegions()
        {
            return _dbContext.Regions.Where(x => x.IsActive).ToList();
        }
    }
}