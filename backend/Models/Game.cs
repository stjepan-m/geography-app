using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class Game
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public int? CreatedBy { get; set; }

    public int Type { get; set; }

    public int ScoringType { get; set; }

    public int Region { get; set; }

    public int NumberOfRounds { get; set; }

    public int? TimeLimitSeconds { get; set; }

    public int TimeLimitType { get; set; }

    public bool AllowSkip { get; set; }

    public bool AllowRetry { get; set; }

    public bool IsFinished { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<GameLocation> GameLocations { get; set; } = new List<GameLocation>();

    public virtual Region? RegionNavigation { get; set; } = null!;

    public virtual ScoringType? ScoringTypeNavigation { get; set; } = null!;

    public virtual TimeLimitType? TimeLimitTypeNavigation { get; set; } = null!;

    public virtual GameType? TypeNavigation { get; set; } = null!;

    public virtual ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();
}
