using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeographyGame.Models;

public partial class Region
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal StartLatitude { get; set; }

    public decimal StartLongitude { get; set; }

    public short StartZoom { get; set; }

    public int MaxDistance { get; set; }

    public string? LabelHr { get; set; }

    public string? LabelEn { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<RegionLocation> RegionLocations { get; set; } = new List<RegionLocation>();

    [JsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
