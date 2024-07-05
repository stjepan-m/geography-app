namespace GeographyGame.ViewModels
{
    public class LocationViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; } = null!;

        public string? Type { get; set; } = null!;

        public string? LabelHr { get; set; }

        public string? LabelEn { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int? Country { get; set; }

        public string? LandAndWaterCoordinates { get; set; } = null!;
    }
}