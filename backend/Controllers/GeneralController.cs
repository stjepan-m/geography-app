using GeographyGame.Models;
using GeographyGame.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace GeographyGame.Controllers
{
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<GameController> _logger;

        public GeneralController(GeographyGameContext dbContext, ILogger<GameController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("gameOptions")]
        public ActionResult<GameOptions> getGameOptions()
        {
            var gameOptions = new GameOptions();
            gameOptions.GameTypes = _dbContext.GameTypes.Where(x => x.IsActive).OrderBy(x => x.Id).ToList();
            gameOptions.Regions = _dbContext.Regions.Where(x => x.IsActive).ToList();

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
            gameOptions.TimeLimitTypes = gameTypeTimeLimitTypes.GroupBy(x => x.GameTypeId, x => x.TimeLimitType).ToDictionary(x => x.Key, x => x.AsEnumerable());

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
            gameOptions.ScoringTypes = gameTypeScoringTypes.GroupBy(x => x.GameTypeId, x => x.ScoringType).ToDictionary(x => x.Key, x => x.AsEnumerable());

            var TimeLimitTypes = _dbContext.TimeLimitTypes.Where(x => x.IsDefault).ToList();
            if (TimeLimitTypes.Count != 1)
            {
                return NotFound();
            }

            gameOptions.DefaultTimeLimitType = TimeLimitTypes.First();

            return gameOptions;
        }

        [HttpGet("gameSet")]
        public ActionResult<IEnumerable<RoundViewModel>> GetGameSet([FromQuery] int? playerGameId, [FromQuery] int? regionId, [FromQuery] int? gameTypeId, [FromQuery] int numberOfRounds = 10)
        {
            try
            {
                if (playerGameId != null)
                {
                    var roundsCount = _dbContext.Rounds.Where(x => x.PlayerGameId == playerGameId).Count();
                    var result = new List<RoundViewModel>();

                    if (roundsCount == 0)
                    {
                        var playerGame = _dbContext.PlayerGames.Find(playerGameId);
                        if (playerGame == null)
                        {
                            return NotFound();
                        }
                        var game = _dbContext.Games.Find(playerGame.GameId);
                        var gameLocations = _dbContext.GameLocations.Where(x => x.GameId == game.Id).Select(x => x.LocationId).ToList();

                        var locationType = _dbContext.Games.Where(x => x.Id == game.Id).Select(x => x.TypeNavigation.LocationType).FirstOrDefault();

                        result = generateGameSet(gameLocations, locationType, game.NumberOfRounds);

                        var roundsToInsert = result.ConvertAll<Round>(x => new Round
                        {
                            PlayerGameId = playerGame.Id,
                            LocationId = x.LocationId ?? 0,
                            RoundNumber = x.RoundNumber ?? 0
                        });

                        _dbContext.Rounds.AddRange(roundsToInsert);
                        _dbContext.SaveChanges();

                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i].Id = roundsToInsert[i].Id;
                            result[i].PlayerGameId = playerGame.Id;
                        }

                        return result;
                    }
                    else
                    {
                        result = _dbContext.Rounds
                            .Where(x => x.PlayerGameId == playerGameId)
                            .OrderBy(x => x.RoundNumber)
                            .Select(x => new RoundViewModel
                            {
                                Id = x.Id,
                                PlayerGameId = x.PlayerGameId,
                                RoundNumber = x.RoundNumber,
                                TimeLeft = x.TimeLeft,
                                Score = x.Score,
                                LocationId = x.LocationId,
                                Location = new LocationViewModel
                                {
                                    Id = x.LocationId,
                                    Name = x.Location == null ? null : x.Location.Name,
                                    Type = x.Location == null ? null : x.Location.Type,
                                    LabelEn = x.Location == null ? null : x.Location.LabelEn,
                                    LabelHr = x.Location == null ? null : x.Location.LabelHr,
                                    Latitude = x.Location == null || x.Location.City == null ? null : x.Location.City.Latitude,
                                    Longitude = x.Location == null || x.Location.City == null ? null : x.Location.City.Longitude,
                                    Country = x.Location == null || x.Location.City == null ? null : x.Location.City.Country,
                                    LandAndWaterCoordinates = x.Location == null || x.Location.Country == null ? null : x.Location.Country.LandAndWaterCoordinates
                                }
                            })
                            .ToList();
                    }

                    return result;
                }
                else if (regionId != null && gameTypeId != null)
                {
                    var region = _dbContext.Regions.Find(regionId);
                    var gameType = _dbContext.GameTypes.Find(gameTypeId);

                    if (region == null || gameType == null)
                    {
                        return NotFound();
                    }

                    var regionLocations = _dbContext.RegionLocations.Where(x => x.RegionId == regionId && x.Location != null && !x.Location.IsCustom).Select(x => x.LocationId).ToList();

                    return generateGameSet(regionLocations, gameType.LocationType, numberOfRounds);
                }
                else
                {
                    return StatusCode(400);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500);

            }

        }

        private List<RoundViewModel> generateGameSet(IEnumerable<int?> locationIds, string locationType, int numberOfRounds)
        {
            var result = new List<RoundViewModel>();
            switch (locationType)
            {
                case "city":
                    result = _dbContext.Cities
                        .Where(x => locationIds.Contains(x.LocationId))
                        .Select(x => new RoundViewModel
                        {
                            LocationId = x.LocationId,
                            Location = new LocationViewModel
                            {
                                Id = x.LocationId,
                                Name = x.Location.Name,
                                Type = x.Location.Type,
                                LabelEn = x.Location.LabelEn,
                                LabelHr = x.Location.LabelHr,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                Country = x.Country
                            }
                        })
                        .ToList();
                    break;
                case "country":
                    result = _dbContext.Countries
                        .Where(x => locationIds.Contains(x.LocationId))
                        .Select(x => new RoundViewModel
                        {
                            LocationId = x.LocationId,
                            Location = new LocationViewModel
                            {
                                Id = x.LocationId,
                                Name = x.Location.Name,
                                Type = x.Location.Type,
                                LabelEn = x.Location.LabelEn,
                                LabelHr = x.Location.LabelHr,
                                LandAndWaterCoordinates = x.LandAndWaterCoordinates
                            }
                        })
                        .ToList();
                    break;
            }

            var rand = new Random();
            result = result.OrderBy(x => rand.Next()).Take(numberOfRounds).ToList();
            foreach (var round in result)
            {
                round.RoundNumber = (short)(result.IndexOf(round) + 1);
            }

            return result;
        }
    }
}