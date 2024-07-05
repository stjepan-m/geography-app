using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class GameTypeScoringType
{
    public int Id { get; set; }

    public int GameTypeId { get; set; }

    public int ScoringTypeId { get; set; }

    public virtual GameType? GameType { get; set; }

    public virtual ScoringType? ScoringType { get; set; }
}
