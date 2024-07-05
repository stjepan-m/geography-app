using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class GameTypeTimeLimitType
{
    public int Id { get; set; }

    public int GameTypeId { get; set; }

    public int TimeLimitTypeId { get; set; }

    public virtual GameType? GameType { get; set; }

    public virtual TimeLimitType? TimeLimitType { get; set; }
}
