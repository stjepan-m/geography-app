using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? LabelHr { get; set; }

    public string? LabelEn { get; set; }

    public bool IsCustom { get; set; }

    public int? CreatedBy { get; set; }

    public virtual City? City { get; set; }

    public virtual Country? Country { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<RegionLocation> RegionLocations { get; set; } = new List<RegionLocation>();

    public virtual ICollection<GameLocation> GameLocations { get; set; } = new List<GameLocation>();

    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();
}
