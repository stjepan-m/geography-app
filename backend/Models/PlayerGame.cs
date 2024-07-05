using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class PlayerGame
{
    public int Id { get; set; }

    public int? PlayerId { get; set; }

    public string? GameId { get; set; }

    public int RoundsCompleted { get; set; }

    public int? TimeLeft { get; set; }

    public float TotalScore { get; set; }

    public string? Status { get; set; }

    public virtual Game? Game { get; set; }

    public virtual Player? Player { get; set; }

    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();
}
