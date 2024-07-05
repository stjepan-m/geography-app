using GeographyGame.Models;

namespace GeographyGame.ViewModels
{
    public class GameOptions
    {
        public IEnumerable<GameType> GameTypes { get; set; } = new List<GameType>();
        public IEnumerable<Region> Regions { get; set; } = new List<Region>();
        public Dictionary<int, IEnumerable<TimeLimitType>> TimeLimitTypes { get; set; } = new Dictionary<int, IEnumerable<TimeLimitType>>();
        public Dictionary<int, IEnumerable<ScoringType>> ScoringTypes { get; set; } = new Dictionary<int, IEnumerable<ScoringType>>();
        public TimeLimitType DefaultTimeLimitType { get; set; } = new TimeLimitType();
    }
}
