<template>
  <div id="map" class="map"></div>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import * as FEATURE_STYLES from 'src/utilities/featureStyles';
import { onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Feature, Map, View } from 'ol';
import { TileWMS, Vector as VectorSource } from 'ol/source';
import { Tile as TileLayer, Vector as VectorLayer } from 'ol/layer';
import { Geometry, LineString, MultiPolygon, Point, Polygon } from 'ol/geom';
import { Zoom, ZoomToExtent, Attribution } from 'ol/control';
import { Style } from 'ol/style';
import { Draw } from 'ol/interaction';
import { Coordinate } from 'ol/coordinate';
import { GeoJSON } from 'ol/format';
import * as turf from '@turf/turf';
import { croatiaCoordinates } from '../assets/croatia-border';
import { europeCoordinates } from '../assets/world-borders';

const { t } = useI18n();

//props
const props = defineProps({
  view: {
    type: String,
    default: APP_CONSTANTS.REGION_WORLD,
    validator(value: string) {
      return APP_CONSTANTS.REGIONS.includes(value);
    },
  },
  zoom: {
    type: Number,
    default: 1,
  },
  startLatitude: {
    type: Number,
    default: 0,
  },
  startLongitude: {
    type: Number,
    default: 0,
  },
  showBorders: {
    type: Boolean,
    default: false,
  },
  interactionType: {
    type: String,
    default: APP_CONSTANTS.INTERACTION_DRAW,
  },
  drawPoints: {
    type: Boolean,
    default: false,
  },
  drawPolygons: {
    type: Boolean,
    default: false,
  },
  matchType: {
    type: String,
  },
});

//data
let mapView: View;
let featureType: string;
let draw: Draw;
let source: VectorSource;
let map: Map;

//emits
const emit = defineEmits({
  mapReady: (isReady) => {
    if (isReady) {
      return true;
    } else {
      console.warn('Invalid submit event payload!');
      return false;
    }
  },
  newFeature: (feature) => {
    if (feature) {
      return true;
    } else {
      console.warn('Invalid submit event payload!');
      return false;
    }
  },
});

//exposed methods
defineExpose({
  /**
   * Removes the interaction ability from the map
   */
  removeInteraction: function () {
    map.removeInteraction(draw);
  },

  /**
   * Adds a specific location point on the map
   * @param label Name of the location being added, to be added as a label on the feature
   * @param coordinates Coordinates of the location (point)
   * @param latestPoint Latest drawn point, to be connected with a line to the new location point
   */
  addLocationPoint: function (label: string, coordinates: number[], latestPoint: Geometry | undefined) {
    confirmUnconfirmedGuesses();

    let pointFeature = new Feature({
      geometry: new Point(coordinates).transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857),
      name: label,
    });
    let style = FEATURE_STYLES.LOCATION_POINT.clone();
    style.getText().setText(pointFeature.get(APP_CONSTANTS.ATTRIBUTE_NAME));
    pointFeature.setStyle(style);

    source.addFeature(pointFeature);
    if (latestPoint) {
      source.addFeature(
        new Feature({
          geometry: new LineString([
            (pointFeature.getGeometry() as Point).getCoordinates(),
            (latestPoint.transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857) as Point).getCoordinates(),
          ]),
        })
      );
    }
  },

  /**
   * Adds a specific location polygon on the map
   * @param label Name of the location being added, to be added as a label on the feature
   * @param coordinates Coordinates of the location (polygon)
   * @param latestPolygon Latest drawn polygon, to be used to draw the difference and intersection with the new polygon
   */
  addLocationPolygon: function (label: string, coordinates: number[][][][], latestPolygon: Polygon | undefined) {
    removeUnconfirmedGuesses();

    let polygon = turf.multiPolygon(coordinates);
    if (latestPolygon) {
      let drawnPolygon = turf.polygon(latestPolygon.getCoordinates());

      let extraArea = turf.difference(drawnPolygon, polygon);
      let missedArea = turf.difference(polygon, drawnPolygon);
      let correctArea = turf.intersect(polygon, drawnPolygon);

      addTurfFeatureToSource(extraArea, FEATURE_STYLES.EXTRA_AREA, `${label} (Extra)`);
      addTurfFeatureToSource(missedArea, FEATURE_STYLES.MISSED_AREA, `${label} (Missed)`);
      addTurfFeatureToSource(correctArea, FEATURE_STYLES.CORRECT_AREA, `${label} (Correct)`);
    } else {
      addTurfFeatureToSource(polygon, FEATURE_STYLES.MISSED_AREA, `${label} (Missed)`);
    }
  },

  /**
   * Adds all the given points to the map for matching
   * @param coordinates Coordinates of points to be added to the map
   */
  addAllPoints: function (coordinates: Array<Array<number>>) {
    coordinates.forEach((coordinate) => {
      addMatchFeature(new Point(coordinate).transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857));
    });
  },

  /**
   * Adds all the given polygons to the map for matching
   * @param coordinates Coordinates of polygons to be added to the map
   */
  addAllPolygons: function (coordinates: Array<number[][][][]>) {
    coordinates.forEach((coordinate) => {
      addMatchFeature(new MultiPolygon(coordinate).transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857));
    });
  },

  /**
   * Marks a correct/incorrect matching guess on the map
   * @param isCorrect Defines whether the guess was correct or incorrect
   * @param geometry Geometry of the guess
   * @param label Name of the guess, to be added as the label of the feature
   */
  markGuess: function (isCorrect: boolean, coordinates: Array<number>, label: string) {
    let newCoordinates;
    if (props.matchType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
      newCoordinates = (new Point(coordinates).transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857) as Point).getCoordinates();
    } else if (props.matchType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
      newCoordinates = (new MultiPolygon(coordinates).transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857) as MultiPolygon)
        .getInteriorPoints()
        .getPoint(0)
        .getCoordinates();
    }

    let feature = source.getFeaturesAtCoordinate(newCoordinates as Coordinate).filter((f) => f.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) !== undefined);
    if (feature.length > 0) {
      handleGuess(feature[0], label, isCorrect);
    }
  },
});

//helper methods
function setFeatureType() {
  if (props.drawPoints) featureType = APP_CONSTANTS.FEATURE_TYPE_POINT;
  else if (props.drawPolygons) featureType = APP_CONSTANTS.FEATURE_TYPE_POLYGON;
  else featureType = APP_CONSTANTS.FEATURE_TYPE_NONE;
}

function addInteraction() {
  if (featureType !== APP_CONSTANTS.FEATURE_TYPE_NONE) {
    draw = new Draw({
      source: source,
      type: featureType,
    });

    map.addInteraction(draw);
  }
}

function fireFeatureEvent(feature: Feature) {
  let geometry: Geometry | undefined = feature?.getGeometry()?.clone().transform(APP_CONSTANTS.MAP_EPSG_3857, APP_CONSTANTS.MAP_EPSG_4326);
  if (geometry !== undefined) {
    emit('newFeature', geometry as Point | MultiPolygon);
  }
}

function addMatchFeature(geometry: Geometry) {
  let feature = new Feature({
    geometry,
    matchStatus: APP_CONSTANTS.MATCH_STATUS_NONE,
  });

  feature.setStyle(getMatchStyle(APP_CONSTANTS.MATCH_STATUS_NONE));
  source.addFeature(feature);
}

function handleSelection(feature: Feature) {
  if (feature.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) === APP_CONSTANTS.MATCH_STATUS_NONE) {
    source
      .getFeatures()
      .filter((f) => f.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) === APP_CONSTANTS.MATCH_STATUS_SELECTED)
      .forEach((f) => {
        f.setStyle([getMatchStyle(APP_CONSTANTS.MATCH_STATUS_NONE)]);
        f.set(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS, APP_CONSTANTS.MATCH_STATUS_NONE);
      });
    feature.setStyle([getMatchStyle(APP_CONSTANTS.MATCH_STATUS_SELECTED)]);
    feature.set(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS, APP_CONSTANTS.MATCH_STATUS_SELECTED);
    fireFeatureEvent(feature);
  }
}

function handleGuess(feature: Feature, label: string, success: boolean) {
  source
    .getFeatures()
    .filter((f) => f.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) === APP_CONSTANTS.MATCH_STATUS_SELECTED)
    .forEach((f) => {
      f.setStyle([getMatchStyle(APP_CONSTANTS.MATCH_STATUS_NONE)]);
      f.set(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS, APP_CONSTANTS.MATCH_STATUS_NONE);
    });

  let style = getMatchStyle(success ? APP_CONSTANTS.MATCH_STATUS_CORRECT : APP_CONSTANTS.MATCH_STATUS_INCORRECT).clone();
  style.getText().setOffsetY(10);
  style.getText().setText(label);
  feature.setStyle([style]);
  feature.set(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS, success ? APP_CONSTANTS.MATCH_STATUS_CORRECT : APP_CONSTANTS.MATCH_STATUS_INCORRECT);
}

function removeUnconfirmedGuesses() {
  source
    .getFeatures()
    .filter((f) => f.get(APP_CONSTANTS.ATTRIBUTE_TYPE) === APP_CONSTANTS.FT_UNCONFIRMED_GUESS)
    .forEach((f) => source.removeFeature(f));
}

function confirmUnconfirmedGuesses() {
  source
    .getFeatures()
    .filter((f) => f.get(APP_CONSTANTS.ATTRIBUTE_TYPE) === APP_CONSTANTS.FT_UNCONFIRMED_GUESS)
    .forEach((f) => {
      f.set(APP_CONSTANTS.ATTRIBUTE_TYPE, APP_CONSTANTS.FT_CONFIRMED_GUESS);
    });
}

function addTurfFeatureToSource(turfFeature: turf.Feature<turf.MultiPolygon | turf.Polygon, turf.Properties> | null, style: Style, label: string) {
  if (turfFeature !== null) {
    let feature = new GeoJSON().readFeature(turfFeature);

    feature.setGeometry(feature.getGeometry()?.clone().transform(APP_CONSTANTS.MAP_EPSG_4326, APP_CONSTANTS.MAP_EPSG_3857));
    feature.set(APP_CONSTANTS.ATTRIBUTE_NAME, label);
    feature.setStyle(style);
    source.addFeature(feature);
  }
}

function addBorderFeature(region: string) {
  if (featureType === APP_CONSTANTS.FEATURE_TYPE_POINT || props.matchType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
    let borderFeature = new Feature({
      geometry: new MultiPolygon(region === APP_CONSTANTS.REGION_CROATIA ? croatiaCoordinates : europeCoordinates).transform(
        APP_CONSTANTS.MAP_EPSG_4326,
        APP_CONSTANTS.MAP_EPSG_3857
      ),
      name: region,
    });
    borderFeature.setStyle(region === APP_CONSTANTS.REGION_CROATIA ? FEATURE_STYLES.CROATIA_BORDER : FEATURE_STYLES.EUROPE_BORDER);
    source.addFeature(borderFeature);
  }
}

function getMatchStyle(matchStatus: string) {
  if (props.matchType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
    switch (matchStatus) {
      case APP_CONSTANTS.MATCH_STATUS_CORRECT:
        return FEATURE_STYLES.LOCATION_ICON_POSITIVE;
      case APP_CONSTANTS.MATCH_STATUS_INCORRECT:
        return FEATURE_STYLES.LOCATION_ICON_NEGATIVE;
      case APP_CONSTANTS.MATCH_STATUS_SELECTED:
        return FEATURE_STYLES.LOCATION_ICON_SELECTED;
      default:
        return FEATURE_STYLES.LOCATION_ICON_NONE;
    }
  } else if (props.matchType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
    switch (matchStatus) {
      case APP_CONSTANTS.MATCH_STATUS_CORRECT:
        return FEATURE_STYLES.MATCH_AREA_POSITIVE;
      case APP_CONSTANTS.MATCH_STATUS_INCORRECT:
        return FEATURE_STYLES.MATCH_AREA_NEGATIVE;
      case APP_CONSTANTS.MATCH_STATUS_SELECTED:
        return FEATURE_STYLES.MATCH_AREA_SELECTED;
      default:
        return FEATURE_STYLES.MATCH_AREA_NONE;
    }
  } else {
    return new Style();
  }
}

//lifecycle hooks
onMounted(() => {
  source = new VectorSource({
    wrapX: false,
  });
  setFeatureType();
  addBorderFeature(props.view);

  if (props.interactionType === APP_CONSTANTS.INTERACTION_DRAW) {
    source.on(APP_CONSTANTS.ADD_FEATURE_EVENT, (event) => {
      if (
        event.feature !== undefined &&
        !(APP_CONSTANTS.ATTRIBUTE_NAME in event.feature.getProperties()) &&
        (event.feature.getGeometry()?.getType() === featureType || event.feature.getGeometry()?.getType() === props.matchType)
      ) {
        removeUnconfirmedGuesses();
        fireFeatureEvent(event.feature);
        if (event.feature.getGeometry()?.getType() === APP_CONSTANTS.FEATURE_TYPE_POINT) {
          event.feature?.setStyle(FEATURE_STYLES.DEFAULT_POINT);
        }
        event.feature?.set(APP_CONSTANTS.ATTRIBUTE_TYPE, APP_CONSTANTS.FT_UNCONFIRMED_GUESS);
      }
    });
  }

  const vector = new VectorLayer({
    source: source,
  });

  const raster = new TileLayer({
    source: new TileWMS({
      url: APP_CONSTANTS.TOPOGRAPHIC_LAYER_URL,
      params: {
        REQUEST: 'GetMap',
        LAYERS: 'TOPO-WMS',
      },
      attributions: ['© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'],
    }),
  });

  mapView = new View({
    center: [props.startLatitude * 100000, props.startLongitude * 100000],
    zoom: props.zoom,
    maxZoom: 11,
  });

  map = new Map({
    target: 'map',
    controls: [new Zoom()],
    layers: [raster, vector],
    view: mapView,
  });

  map.addControl(
    new ZoomToExtent({
      label: '',
      tipLabel: t('recenterMessage'),
      extent: mapView.calculateExtent(map.getSize()),
    })
  );

  map.addControl(
    new Attribution({
      collapsible: false,
      collapsed: false,
    })
  );

  addInteraction();

  if (props.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
    map.on('pointermove', (event) => {
      const pixel = map.getEventPixel(event.originalEvent);
      const hit = map.forEachFeatureAtPixel(pixel, (feature) => {
        if (feature.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) === APP_CONSTANTS.MATCH_STATUS_NONE) {
          return true;
        }
        return false;
      });

      map.getTargetElement().style.cursor = hit ? 'pointer' : '';
    });

    map.on('click', (event) => {
      const feature = map.forEachFeatureAtPixel(event.pixel, (feature) => {
        if (feature.get(APP_CONSTANTS.ATTRIBUTE_MATCH_STATUS) === APP_CONSTANTS.MATCH_STATUS_NONE) {
          return feature;
        }
      });
      if (feature) {
        handleSelection(feature as Feature);
      }
    });
  }

  emit('mapReady', true);
});
</script>

<style>
@import url('../../node_modules/ol/ol.css');
.map {
  width: 100%;
  height: 90vh;
}

.ol-overlaycontainer-stopevent {
  z-index: 8000 !important;
}

.ol-zoom-extent button {
  background-image: url('../assets/recenter.svg');
  background-repeat: no-repeat;
}
</style>
