using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class Round
{
    public int Id { get; set; }

    public int PlayerGameId { get; set; }

    public int LocationId { get; set; }

    public short RoundNumber { get; set; }

    public int? TimeLeft { get; set; }

    public float? Score { get; set; }

    public virtual Location? Location { get; set; }

    public virtual PlayerGame? PlayerGame { get; set; }
}
