using GeographyGame.Models;

namespace GeographyGame.ViewModels
{
    public class CountryViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; } = null!;

        public string? Type { get; set; } = null!;

        public string? LabelHr { get; set; }

        public string? LabelEn { get; set; }

        public bool? IsCustom { get; set; }

        public string? CountryCode { get; set; } = null!;

        public string? LandAndWaterCoordinates { get; set; } = null!;

        public IEnumerable<RegionLocation> Regions { get; set; } = new List<RegionLocation>();
    }
}