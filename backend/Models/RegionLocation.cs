using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class RegionLocation
{
    public int Id { get; set; }

    public int? LocationId { get; set; }

    public int? RegionId { get; set; }

    public virtual Location? Location { get; set; }

    public virtual Region? Region { get; set; }
}
