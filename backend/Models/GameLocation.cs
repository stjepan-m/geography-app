using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class GameLocation
{
    public int Id { get; set; }

    public string? GameId { get; set; }

    public int? LocationId { get; set; }

    public virtual Location? Location { get; set; }

    public virtual Game? Game { get; set; }
}
