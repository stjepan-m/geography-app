using GeographyGame.Filters;
using GeographyGame.Models;
using GeographyGame.Util;
using GeographyGame.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GeographyGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoundController : ControllerBase
    {
        private readonly GeographyGameContext _dbContext;
        private readonly ILogger<RoundController> _logger;

        public RoundController(GeographyGameContext dbContext, ILogger<RoundController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet]
        public ActionResult<IEnumerable<Round>> GetRounds()
        {
            return _dbContext.Rounds.ToList();
        }

        [ServiceFilter(typeof(AdminOrTeacherAuthorizationFilter))]
        [HttpGet("{id}")]
        public ActionResult<Round> GetRoundById(int id)
        {
            var Round = _dbContext.Rounds.Find(id);
            if (Round == null)
            {
                return NotFound();
            }
            return Round;
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPost]
        public ActionResult<Round> CreateRound([FromBody] Round Round)
        {
            _dbContext.Rounds.Add(Round);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetRoundById), new { id = Round.Id }, Round);
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpPut("{id}")]
        public IActionResult UpdateRound(int id, [FromBody] Round updatedRound)
        {
            var Round = _dbContext.Rounds.Find(id);
            if (Round == null)
            {
                return NotFound();
            }

            _dbContext.SaveChanges();
            return NoContent();
        }

        [ServiceFilter(typeof(AdminAuthorizationFilter))]
        [HttpDelete("{id}")]
        public IActionResult DeleteRound(int id)
        {
            var Round = _dbContext.Rounds.Find(id);
            if (Round == null)
            {
                return NotFound();
            }

            _dbContext.Rounds.Remove(Round);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPut("calculateScore")]
        public ActionResult<int> CalculateScore([FromBody] RoundForCalculation data)
        {
            if (data == null || data.round == null || data.gameType == null || data.region == null)
            {
                return StatusCode(400);
            }

            var result = 0;

            if (data.gameType.FeatureType == AppConstants.FEATURE_TYPE_POINT && data.distance != null)
            {
                if (data.gameType.InteractionType == AppConstants.INTERACTION_MATCH)
                {
                    switch (data.scoringType == null ? null : data.scoringType.Name)
                    {
                        default:
                            result = data.distance == 0 ? 1 : 0;
                            break;
                    }
                }
                else if (data.gameType.InteractionType == AppConstants.INTERACTION_DRAW)
                {
                    var maxDistance = data.region.MaxDistance;
                    
                    var distanceScore = 0.0;

                    switch (data.scoringType == null ? null : data.scoringType.Name)
                    {
                        case AppConstants.SCORING_TYPE_SQUARE:
                            maxDistance /= 1000;
                            data.distance /= 1000;
                            distanceScore = maxDistance * maxDistance - (double)(data.distance * data.distance);
                            result = distanceScore < 0 ? 0 : (int)Math.Round(((distanceScore) / (maxDistance * maxDistance)) * 100);
                            break;
                        default:
                            distanceScore = maxDistance * 1.05 - (double)data.distance; // 5% margin for error because the correct locations are points and not the whole area
                            result = distanceScore < 0 ? 0 : (distanceScore > maxDistance ? 100 : (int)Math.Round(((distanceScore) / maxDistance) * 100));
                            break;
                    }
                }
            }
            else if(data.gameType.FeatureType == AppConstants.FEATURE_TYPE_POLYGON && data.correctArea != null && data.missedArea != null && data.extraArea != null)
            {
                if (data.gameType.InteractionType == AppConstants.INTERACTION_MATCH)
                {
                    switch (data.scoringType == null ? null : data.scoringType.Name)
                    {
                        default:
                            result = data.missedArea == 0 ? 1 : 0;
                            break;
                    }
                }
                else if (data.gameType.InteractionType == AppConstants.INTERACTION_DRAW)
                {
                    var areaScore = 0.0;

                    switch (data.scoringType == null ? null : data.scoringType.Name)
                    {
                        default:
                            areaScore = Math.Round((double)data.correctArea * 1.05 / (double)(data.correctArea + (data.missedArea + data.extraArea) / 2) * 100);
                            result = areaScore > 100 ? 100 : (int)areaScore;
                            break;
                    }
                }
            }

            if (data.round.Id != null)
            {
                var Round = _dbContext.Rounds.Find(data.round.Id);
                if (Round == null)
                {
                    return NotFound();
                }
                Round.Score = result;
                Round.TimeLeft = null;

                var playerGame = _dbContext.PlayerGames.Find(Round.PlayerGameId);
                playerGame.RoundsCompleted++;
                playerGame.TotalScore += result;

                _dbContext.SaveChanges();
            }

            return result;
        }

        [HttpPut("{id}/setTimeLeft")]
        public IActionResult SetTimeLeft(int id, [FromBody] int timeLeft)
        {
            var Round = _dbContext.Rounds.Find(id);
            if (Round == null)
            {
                return NotFound();
            }

            Round.TimeLeft = timeLeft;
            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPost("{id}/suspend")]
        public IActionResult Suspend(int id, [FromQuery] int timeLeft)
        {
            var Round = _dbContext.Rounds.Find(id);
            if (Round == null)
            {
                return NotFound();
            }

            Round.TimeLeft = timeLeft;
            _dbContext.SaveChanges();
            return NoContent();
        }


        public class RoundForCalculation
        {
            public RoundViewModel? round { get; set; }
            public double? distance { get; set; }
            public double? correctArea { get; set; }
            public double? missedArea { get; set; }
            public double? extraArea { get; set; }
            public GameType? gameType { get; set; }
            public Region? region { get; set; }
            public ScoringType? scoringType { get; set; }
            public TimeLimitType? timeLimitType { get; set; }
        }
    }
}