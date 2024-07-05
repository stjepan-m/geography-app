using GeographyGame.Models;
using GeographyGame.ViewModels;
using GeographyGame.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeographyGame.Util;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<CityController> _logger;

        public CityController(GeographyGameContext dbContext, ILogger<CityController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<CityViewModel>> GetCities([FromQuery] string? ids, [FromQuery] int? region, [FromQuery] string? game, [FromQuery] string? orderBy)
        {
            if (HttpContext.Items["User"] is User user)
            {
                try
                {
                    var idsArray = ids?.Split(',').Select(int.Parse).ToArray();
                    var regionLocations = _dbContext.RegionLocations.Where(x => x.RegionId == region).Select(x => x.LocationId).ToList();
                    var gameLocations = _dbContext.GameLocations.Where(x => x.GameId == game).Select(x => x.LocationId).ToList();
                    var result = _dbContext.Cities
                        .Where(x => (user.UserType == AppConstants.USER_TYPE_ADMINISTRATOR || !x.Location.IsCustom || x.Location.CreatedBy == user.Id) && (idsArray == null || idsArray.Contains(x.LocationId)) && (region == null || regionLocations.Contains(x.LocationId)) && (game == null || gameLocations.Contains(x.LocationId)))
                        .Select(x => new CityViewModel
                        {
                            Id = x.LocationId,
                            Name = x.Location.Name,
                            Type = x.Location.Type,
                            LabelHr = x.Location.LabelHr,
                            LabelEn = x.Location.LabelEn,
                            IsCustom = x.Location.IsCustom,
                            Latitude = x.Latitude,
                            Longitude = x.Longitude,
                            Country = x.CountryNavigation != null ? new CountryViewModel
                            {
                                Id = x.CountryNavigation.LocationId,
                                Name = x.CountryNavigation.Location.Name,
                                Type = x.CountryNavigation.Location.Type,
                                LabelHr = x.CountryNavigation.Location.LabelHr,
                                LabelEn = x.CountryNavigation.Location.LabelEn,
                                CountryCode = x.CountryNavigation.CountryCode,
                                LandAndWaterCoordinates = x.CountryNavigation.LandAndWaterCoordinates,
                                Regions = x.CountryNavigation.Location.RegionLocations
                            } : null,
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
        public ActionResult<City> GetCityById(int id)
        {
            var City = _dbContext.Cities.Find(id);
            if (City == null)
            {
                return NotFound();
            }
            return City;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPost]
        public ActionResult<City> CreateCity([FromBody] CityViewModel City)
        {
            if (HttpContext.Items["User"] is User user)
            {
                using var transaction = _dbContext.Database.BeginTransaction();

                try
                {
                    var location = new Location
                    {
                        Name = City.Name,
                        Type = AppConstants.LOCATION_CITY.ToLower(),
                        LabelEn = City.LabelEn,
                        LabelHr = City.LabelHr,
                        IsCustom = true,
                        CreatedBy = user.Id
                    };
                    _dbContext.Locations.Add(location);
                    _dbContext.SaveChanges();

                    _dbContext.Cities.Add(new City
                    {
                        LocationId = location.Id,
                        Latitude = City.Latitude,
                        Longitude = City.Longitude,
                        Country = City.Country.Id
                    });
                    foreach (var region in City.Regions)
                    {
                        _dbContext.RegionLocations.Add(new RegionLocation { LocationId = location.Id, RegionId = region.RegionId });
                    }
                    _dbContext.SaveChanges();

                    transaction.Commit();
                    return CreatedAtAction(nameof(GetCityById), new { id = location.Id }, City);

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
        [HttpDelete("{id}")]
        public IActionResult DeleteCity(int id)
        {
            var City = _dbContext.Cities.Find(id);
            if (City == null)
            {
                return NotFound();
            }

            _dbContext.Cities.Remove(City);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpPost("import")]
        public ActionResult<int> ImportCities([FromBody] IEnumerable<CityViewModel> Cities)
        {
            if (HttpContext.Items["User"] is User user)
            {
                var customCities = Cities.Where(x => x.IsCustom);
                var existingCities = _dbContext.Cities
                    .Where(x => x.Location.IsCustom && (user.UserType == AppConstants.USER_TYPE_ADMINISTRATOR || x.Location.CreatedBy == user.Id))
                    .Include(x => x.Location)
                    .ToList()
                    .GroupBy(p => p.Location.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.AsEnumerable());
                using var transaction = _dbContext.Database.BeginTransaction();

                try
                {
                    var newCityCounter = 0;
                    foreach (var city in customCities)
                    {
                        existingCities.TryGetValue(city.Name, out IEnumerable<City>? existingCitiesByName);
                        var cityFound = false;
                        if (existingCitiesByName != null)
                        {
                            foreach (var existingCity in existingCitiesByName)
                            {
                                if (existingCity.Country == city.Country.Id && existingCity.Latitude == city.Latitude && existingCity.Longitude == city.Longitude)
                                {
                                    cityFound = true;
                                    break;
                                }
                            }
                        }
                        if(!cityFound)
                        {
                            var location = new Location
                            {
                                Name = city.Name,
                                Type = AppConstants.LOCATION_CITY.ToLower(),
                                LabelEn = city.LabelEn,
                                LabelHr = city.LabelHr,
                                IsCustom = true,
                                CreatedBy = user.Id
                            };
                            _dbContext.Locations.Add(location);
                            _dbContext.SaveChanges();

                            _dbContext.Cities.Add(new City
                            {
                                LocationId = location.Id,
                                Latitude = city.Latitude,
                                Longitude = city.Longitude,
                                Country = city.Country.Id
                            });
                            foreach (var region in city.Regions)
                            {
                                _dbContext.RegionLocations.Add(new RegionLocation { LocationId = location.Id, RegionId = region.RegionId });
                            }
                            _dbContext.SaveChanges();
                            newCityCounter++;
                        }
                    }

                    transaction.Commit();
                    return newCityCounter;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StatusCode(500);
                }
            }
            return Unauthorized();
        }
    }
}