//Game constants
export const GAME_STATUS_NOT_STARTED = 'Not Started';
export const GAME_STATUS_IN_PROGRESS = 'In Progress';
export const GAME_STATUS_SUSPENDED = 'Suspended';
export const GAME_STATUS_CANCELLED = 'Cancelled';
export const GAME_STATUS_COMPLETED = 'Completed';

//Region constants
export const REGIONS = ['World', 'Europe', 'Croatia'];
export const REGION_WORLD = 'World';
export const REGION_EUROPE = 'Europe';
export const REGION_CROATIA = 'Croatia';

//Location constants
export const LOCATION_COUNTRY = 'Country';
export const LOCATION_CITY = 'City';
export const LOCATION_LOCATION = 'Location';
export const POINT_LOCATIONS = ['City'];
export const POLYGON_LOCATIONS = ['Country'];
export const LOCATION_SELECTION_RANDOM = 'random';
export const LOCATION_SELECTION_SELECT = 'select';

//Interaction constants
export const INTERACTIONS = ['Draw', 'Match'];
export const INTERACTION_DRAW = 'Draw';
export const INTERACTION_MATCH = 'Match';

//Map constants
export const FEATURE_TYPES = ['Point', 'Polygon', 'MultiPolygon'];
export const FEATURE_TYPES_POLYGON = ['Polygon', 'MultiPolygon'];
export const FEATURE_TYPE_POINT = 'Point';
export const FEATURE_TYPE_POLYGON = 'Polygon';
export const FEATURE_TYPE_MULTIPOLYGON = 'MultiPolygon';
export const FEATURE_TYPE_NONE = 'None';
export const FT_UNCONFIRMED_GUESS = 'Unconfirmed Guess';
export const FT_CONFIRMED_GUESS = 'Confirmed Guess';
export const MAP_EPSG_3857 = 'EPSG:3857';
export const MAP_EPSG_4326 = 'EPSG:4326';
export const ATTRIBUTE_NAME = 'name';
export const ATTRIBUTE_TYPE = 'type';
export const ATTRIBUTE_MATCH_STATUS = 'matchStatus';
export const MATCH_STATUS_NONE = 'None';
export const MATCH_STATUS_SELECTED = 'Selected';
export const MATCH_STATUS_CORRECT = 'Correct';
export const MATCH_STATUS_INCORRECT = 'Incorrect';

//Time Limit Constants
export const TIME_LIMIT_TYPE_TOTAL = 'Total';
export const TIME_LIMIT_TYPE_PER_ROUND = 'Per Round';

//Style Constants
export const COLOR_PRIMARY = 'primary';
export const COLOR_SECONDARY = 'secondary';
export const COLOR_POSITIVE = 'positive';
export const COLOR_NEGATIVE = 'negative';
export const COLOR_WHITE = 'white';
export const COLOR_GREY_1 = 'grey-1';
export const COLOR_GREEN = 'green';
export const COLOR_ORANGE = 'orange';
export const COLOR_RED = 'red';
export const COLOR_GREEN_FADED = 'green-3';
export const COLOR_ORANGE_FADED = 'orange-3';
export const COLOR_RED_FADED = 'red-3';

//Language Constants
export const LANGUAGE_OPTIONS = [
  { value: 'en-US', label: 'English' },
  { value: 'hr', label: 'Hrvatski' },
];

//OpenRouteService Constants
export const LAYER_LOCALITY = 'locality';

//OpenLayers Constants
export const ADD_FEATURE_EVENT = 'addfeature';
export const TOPOGRAPHIC_LAYER_URL = 'https://ows.terrestris.de/osm/service';

//File Constants
export const FILE_JSON = 'application/json';
