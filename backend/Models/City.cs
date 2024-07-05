using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class City
{
    public int LocationId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public int Country { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Country? CountryNavigation { get; set; } = null!;
}
