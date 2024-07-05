using GeographyGame.Models;

namespace GeographyGame.ViewModels
{
    public class CityViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Type { get; set; } = null!;

        public string? LabelHr { get; set; }

        public string? LabelEn { get; set; }

        public bool IsCustom { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public CountryViewModel Country { get; set; } = null!;

        public IEnumerable<RegionLocation> Regions { get; set; } = new List<RegionLocation>();
    }
}