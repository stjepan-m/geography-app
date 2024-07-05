using System;
using System.Collections.Generic;

namespace GeographyGame.Models;

public partial class Player
{
    public int Id { get; set; }

    public string Nickname { get; set; } = null!;

    public int? UserId { get; set; }

    public virtual ICollection<PlayerGame> PlayerGames { get; set; } = new List<PlayerGame>();

    public virtual User? User { get; set; }
}
