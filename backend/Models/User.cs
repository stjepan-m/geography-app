using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeographyGame.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string GoogleId { get; set; } = null!;

    public string UserType { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

    [JsonIgnore]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
