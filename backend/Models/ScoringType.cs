using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeographyGame.Models;

public partial class ScoringType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Formula { get; set; } = null!;

    public float MaxScore { get; set; }

    public string? LabelHr { get; set; }

    public string? LabelEn { get; set; }

    public bool IsActive { get; set; }

    [JsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<GameTypeScoringType> GameTypeScoringTypes { get; set; } = new List<GameTypeScoringType>();
}
