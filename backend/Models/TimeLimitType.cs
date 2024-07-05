using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeographyGame.Models;

public partial class TimeLimitType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LabelHr { get; set; }

    public string? LabelEn { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    [JsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<GameTypeTimeLimitType> GameTypeTimeLimitTypes { get; set; } = new List<GameTypeTimeLimitType>();
}
