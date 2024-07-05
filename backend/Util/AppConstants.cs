using System.CodeDom;

namespace GeographyGame.Util
{
    public static class AppConstants
    {
        public const string USER_TYPE_ADMINISTRATOR = "Administrator";
        public const string USER_TYPE_TEACHER = "Teacher";
        public const string USER_TYPE_PLAYER = "Player";

        public const string LOCATION_CITY = "City";
        public const string LOCATION_COUNTRY = "Country";

        public const string GAME_STATUS_NOT_STARTED = "Not Started";
        public const string GAME_STATUS_IN_PROGRESS = "In Progress";
        public const string GAME_STATUS_SUSPENDED = "Suspended";
        public const string GAME_STATUS_CANCELLED = "Cancelled";
        public const string GAME_STATUS_COMPLETED = "Completed";

        public const string INTERACTION_DRAW = "Draw";
        public const string INTERACTION_MATCH = "Match";

        public const string FEATURE_TYPE_POINT = "Point";
        public const string FEATURE_TYPE_POLYGON = "Polygon";

        public const string SCORING_TYPE_SQUARE = "Square";
    }
}
