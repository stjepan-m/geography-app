import { getCssVar } from 'quasar';
import { Circle, Fill, Stroke, Style, Text, Icon } from 'ol/style.js';
import locationIcon from '../assets/location.svg';

//Fills
function getDefaultText() {
  return new Text({
    font: 'bold 13px "Roboto", "-apple-system", "Helvetica Neue", Helvetica, Arial, sans-serif',
    placement: 'point',
    offsetY: 15,
    fill: new Fill({
      color: 'black',
    }),
  });
}
function getFill(color: string) {
  return new Fill({
    color,
  });
}

function getStroke(color: string | undefined) {
  return new Stroke({
    color,
  });
}

function getThinStroke(color: string) {
  return new Stroke({
    color,
    width: 1,
  });
}

function getLocationIcon(color: string) {
  return new Icon({
    scale: 0.05,
    anchor: [0.5, 0.88],
    color: getCssVar(color)?.toString(),
    src: locationIcon,
  });
}

function getLowerOpacityColor(color: string) {
  const r = parseInt(getCssVar(color)?.toString().slice(1, 3) as string, 16);
  const g = parseInt(getCssVar(color)?.toString().slice(3, 5) as string, 16);
  const b = parseInt(getCssVar(color)?.toString().slice(5, 7) as string, 16);

  return `rgba(${r}, ${g}, ${b}, 0.5)`;
}

export const CROATIA_BORDER = new Style({
  stroke: getThinStroke('rgb(100, 100, 100)'),
  fill: getFill('rgb(255, 255, 255, 0.2)'),
});

export const EUROPE_BORDER = new Style({
  stroke: getThinStroke('rgb(100, 100, 100)'),
});

export const DEFAULT_POINT = new Style({
  image: new Circle({
    fill: getFill('#02b7b4'),
    stroke: getStroke('white'),
    radius: 6,
  }),
});

export const LOCATION_POINT = new Style({
  image: new Circle({
    fill: getFill('red'),
    stroke: getStroke('white'),
    radius: 6,
  }),
  text: getDefaultText(),
});

export const CORRECT_AREA = new Style({
  fill: getFill('rgba(0, 255, 0, 0.5)'),
  stroke: getStroke('green'),
});

export const MISSED_AREA = new Style({
  fill: getFill('rgba(255, 255, 0, 0.5)'),
  stroke: getStroke('yellow'),
});

export const EXTRA_AREA = new Style({
  fill: getFill('rgba(255, 0, 0, 0.5)'),
  stroke: getStroke('red'),
});

export const LOCATION_ICON_POSITIVE = new Style({
  image: getLocationIcon('positive'),
  text: getDefaultText(),
});

export const LOCATION_ICON_NEGATIVE = new Style({
  image: getLocationIcon('negative'),
  text: getDefaultText(),
});

export const LOCATION_ICON_SELECTED = new Style({
  image: getLocationIcon('primary'),
});

export const LOCATION_ICON_NONE = new Style({
  image: getLocationIcon('secondary'),
});

export const MATCH_AREA_POSITIVE = new Style({
  fill: getFill(getLowerOpacityColor('positive')),
  stroke: getStroke(getCssVar('positive')?.toString()),
  text: getDefaultText(),
});

export const MATCH_AREA_NEGATIVE = new Style({
  fill: getFill(getLowerOpacityColor('negative')),
  stroke: getStroke(getCssVar('negative')?.toString()),
  text: getDefaultText(),
});

export const MATCH_AREA_SELECTED = new Style({
  fill: getFill(getLowerOpacityColor('primary')),
  stroke: getStroke(getCssVar('primary')?.toString()),
});

export const MATCH_AREA_NONE = new Style({
  fill: getFill(getLowerOpacityColor('secondary')),
  stroke: getStroke(getCssVar('secondary')?.toString()),
});
