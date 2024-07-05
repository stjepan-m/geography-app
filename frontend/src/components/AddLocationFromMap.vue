<template>
  <div class="column relative-position col items-center">
    <div id="map" class="map"></div>
    <div
      v-if="currentFeature"
      class="absolute-bottom z-high text-center text-h6 q-py-sm bg-faded column justify-center items-center"
      :class="currentFeatureLabel ? ' text-bold' : 'text-secondary text-weight-regular'"
    >
      <span>{{ currentFeatureLabel ? currentFeatureLabel : t(`no${props.locationType}Found`) }}</span>
      <q-btn
        size="md"
        rounded
        :color="APP_CONSTANTS.COLOR_POSITIVE"
        :label="t(`add${currentFeatureLabel ? props.locationType : 'Manually'}`)"
        class="q-my-sm z-high"
        @click="openAddLocationManual"
      />
    </div>
  </div>
  <q-dialog v-model="addLocationManualDialog">
    <q-card><add-location :location="currentLocation" :location-type="locationType" :countries="countries" :regions="regions" @new-location="handleNewLocation" /></q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import * as FEATURE_STYLES from 'src/utilities/featureStyles';
import { onMounted, ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { reverseGeocode } from 'src/utilities/functions';
import { Location, City, Country, Region } from 'src/models/models';
import { Map as OLMap, View } from 'ol';
import { OSM, Vector as VectorSource } from 'ol/source';
import { Tile as TileLayer, Vector as VectorLayer } from 'ol/layer';
import { Point } from 'ol/geom';
import { Zoom, ZoomToExtent, Attribution } from 'ol/control';
import { Draw } from 'ol/interaction';
import AddLocation from './AddLocation.vue';

//data
const { t } = useI18n();
const q = useQuasar();
let mapView: View;
let source: VectorSource;
let map: OLMap;
let countryMap = new Map<string, Country>();

//props
const props = defineProps({
  locationType: {
    type: String,
    default: APP_CONSTANTS.LOCATION_CITY,
  },
  countries: {
    type: Array<Country>,
    default: [],
  },
  regions: {
    type: Array<Region>,
    default: [],
  },
});

//emits
const emit = defineEmits({
  newLocation: (location) => {
    if (location) {
      return true;
    } else {
      console.warn('Invalid submit event payload!');
      return false;
    }
  },
});

//state
let currentFeature = ref<any | undefined>();
let currentLocation = ref<Location | undefined>();
let addLocationManualDialog = ref(false);

//computed
const currentFeatureLabel = computed(() => {
  if (currentFeature.value) {
    if (props.locationType === APP_CONSTANTS.LOCATION_CITY && currentFeature.value.properties.locality) {
      let country = countryMap.get(currentFeature.value.properties.country_a);
      return currentFeature.value.properties.locality + ', ' + (country !== undefined ? country[t('labelColumn')] : '');
    } else {
      return undefined;
    }
  } else {
    return undefined;
  }
});

//methods
function openAddLocationManual() {
  addLocationManualDialog.value = true;
}

function closeAddLocationManual() {
  addLocationManualDialog.value = false;
}

function handleNewLocation(location: Location) {
  closeAddLocationManual();
  emit('newLocation', location);
}

//helpers
function createCountriesMap() {
  props.countries.forEach((country) => {
    countryMap.set(country.countryCode, country);
  });
}

//lifecycle hooks
onMounted(() => {
  source = new VectorSource({
    wrapX: false,
  });

  const vector = new VectorLayer({
    source: source,
  });

  const raster = new TileLayer({
    source: new OSM(),
  });

  mapView = new View({ center: [0, 0], zoom: 3, maxZoom: 20 });

  map = new OLMap({
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

  source.on(APP_CONSTANTS.ADD_FEATURE_EVENT, async (event) => {
    if (props.locationType === APP_CONSTANTS.LOCATION_CITY) {
      event.feature?.setStyle(FEATURE_STYLES.DEFAULT_POINT);
      let result = await reverseGeocode(
        (event.feature?.getGeometry()?.clone().transform(APP_CONSTANTS.MAP_EPSG_3857, APP_CONSTANTS.MAP_EPSG_4326) as Point).getCoordinates(),
        APP_CONSTANTS.LOCATION_CITY,
        t('retrieveError'),
        q
      );
      if (result.features.length) {
        currentFeature.value = result.features[0];
        currentLocation.value = {
          name: currentFeature.value.properties.locality,
          labelEn: currentFeature.value.properties.locality,
          labelHr: currentFeature.value.properties.locality,
          longitude: currentFeature.value.geometry.coordinates[0],
          latitude: currentFeature.value.geometry.coordinates[1],
          country: countryMap.get(currentFeature.value.properties.country_a),
        } as City;
      }
    }

    source.getFeatures().forEach((feature) => {
      if (feature !== event.feature) {
        source.removeFeature(feature);
      }
    });
  });

  map.addInteraction(
    new Draw({
      source: source,
      type: APP_CONSTANTS.FEATURE_TYPE_POINT,
    })
  );

  createCountriesMap();
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

.bg-faded {
  background-color: rgb(255, 255, 255, 0.8);
}

.z-high {
  z-index: 99;
}
</style>
