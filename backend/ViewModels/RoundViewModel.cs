using GeographyGame.ViewModels;

namespace GeographyGame.Models;

public partial class RoundViewModel
{
    public int? Id { get; set; }

    public int? PlayerGameId { get; set; }

    public int? LocationId { get; set; }

    public short? RoundNumber { get; set; }

    public int? TimeLeft { get; set; }

    public float? Score { get; set; }

    public virtual LocationViewModel? Location { get; set; }
}
