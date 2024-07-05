using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class Country
{
    public int LocationId { get; set; }

    public string CountryCode { get; set; } = null!;

    public string LandAndWaterCoordinates { get; set; } = null!;

    public virtual Location Location { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
