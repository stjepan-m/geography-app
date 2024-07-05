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
    public class CountryController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<CountryController> _logger;

        public CountryController(GeographyGameContext dbContext, ILogger<CountryController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<CountryViewModel>> GetCountries([FromQuery] string? ids, [FromQuery] int? region, [FromQuery] string? game, [FromQuery] string? orderBy)
        {
            if (HttpContext.Items["User"] is User user)
            {
                try
                {
                    var idsArray = ids?.Split(',').Select(int.Parse).ToArray();
                    var regionLocations = _dbContext.RegionLocations.Where(x => x.RegionId == region).Select(x => x.LocationId).ToList();
                    var gameLocations = _dbContext.GameLocations.Where(x => x.GameId == game).Select(x => x.LocationId).ToList();
                    var result = _dbContext.Countries
                        .Where(x => (user.UserType == AppConstants.USER_TYPE_ADMINISTRATOR || !x.Location.IsCustom || x.Location.CreatedBy == user.Id) && (idsArray == null || idsArray.Contains(x.LocationId)) && (region == null || regionLocations.Contains(x.LocationId)) && (game == null || gameLocations.Contains(x.LocationId)))
                        .Select(x => new CountryViewModel
                        {
                            Id = x.LocationId,
                            Name = x.Location.Name,
                            Type = x.Location.Type,
                            LabelHr = x.Location.LabelHr,
                            LabelEn = x.Location.LabelEn,
                            IsCustom = x.Location.IsCustom,
                            CountryCode = x.CountryCode,
                            LandAndWaterCoordinates = x.LandAndWaterCoordinates,
                            Regions = x.Location.RegionLocations
                        })
                        .OrderBy(x => orderBy == null ? x.Name : EF.Property<object>(x, orderBy.Substring(0, 1).ToUpper() + orderBy.Substring(1)))
                        .ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500);
                }
            }
            return Unauthorized();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<Country> GetCountryById(int id)
        {
            var Country = _dbContext.Countries.Find(id);
            if (Country == null)
            {
                return NotFound();
            }
            return Country;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPost]
        public ActionResult<Country> CreateCountry([FromBody] Country Country)
        {
            _dbContext.Countries.Add(Country);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetCountryById), new { id = Country.LocationId }, Country);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteCountry(int id)
        {
            var Country = _dbContext.Countries.Find(id);
            if (Country == null)
            {
                return NotFound();
            }

            _dbContext.Countries.Remove(Country);
            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}