<template>
  <q-table
    v-if="dataLoaded"
    :rows="filteredLocations"
    :columns="columns"
    :visible-columns="visibleColumns"
    virtual-scroll
    :pagination="{ rowsPerPage: 0 }"
    :rows-per-page-options="[0]"
    row-key="id"
    selection="multiple"
    v-model:selected="selectedLocations"
    :selected-rows-label="(numberOfRows) => `${numberOfRows} ${t(locationType.toLowerCase() + 'SelectedNumber')}`"
    class="wide-table q-ma-sm"
  >
    <template v-slot:top>
      <q-select
        v-model="region"
        :options="regions"
        :option-label="t('labelColumn')"
        :label="t('selectRegion')"
        clearable
        class="q-ml-md"
        style="width: 200px"
        @update:model-value="filterByRegion"
        @clear="filterByRegion"
      />
      <q-space />
      <q-btn
        v-if="selectedLocations.length"
        :label="t(`${locationType.toLowerCase()}ExportSelected`)"
        outline
        :color="APP_CONSTANTS.COLOR_PRIMARY"
        icon="archive"
        @click="exportLocations"
      />
      <q-btn-dropdown v-if="locationType === APP_CONSTANTS.LOCATION_CITY" :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t(`add${props.locationType}`)" class="q-ml-sm">
        <q-list>
          <q-item clickable v-close-popup @click="openAddLocationFromMap">
            <q-item-section>
              <q-item-label> <q-icon name="add" class="q-mr-md" />{{ t('fromTheMap') }}</q-item-label>
            </q-item-section>
          </q-item>
          <q-item clickable v-close-popup @click="openAddLocationManual">
            <q-item-section>
              <q-item-label><q-icon name="add" class="q-mr-md" />{{ t('manually') }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
      </q-btn-dropdown>
      <q-btn
        outline
        :color="APP_CONSTANTS.COLOR_PRIMARY"
        @click="openLocationsFileInput"
        icon-right="unarchive"
        :label="t(`${props.locationType.toLowerCase()}Import`)"
        class="q-ml-sm"
      />
      <input id="locationsImport" type="file" ref="fileInputLocationsRef" accept=".json" @change="handleFileUpload" style="display: none" />
    </template>
  </q-table>
  <q-dialog v-model="addLocationManualDialog">
    <q-card><add-location :location-type="locationType" :countries="allCountries" :regions="regions" @new-location="handleNewLocation" /></q-card>
  </q-dialog>
  <q-dialog v-model="addLocationFromMapDialog" full-width>
    <q-card><add-location-from-map :countries="allCountries" :regions="regions" @new-location="handleNewLocationFromMap" /></q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { computed, ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { Location, City, Country, Region, MessageType } from 'src/models/models';
import { retrieveData, exportJSON, showNotification, insertData } from 'src/utilities/functions';
import AddLocation from './AddLocation.vue';
import AddLocationFromMap from './AddLocationFromMap.vue';

//props
const props = defineProps({
  locations: {
    type: Array<Location>,
    default: [],
  },
  locationType: {
    type: String,
    default: APP_CONSTANTS.LOCATION_CITY,
    validator(value: string) {
      return [APP_CONSTANTS.LOCATION_CITY, APP_CONSTANTS.LOCATION_COUNTRY].includes(value);
    },
  },
  allCountries: {
    type: Array<Country>,
    default: [],
  },
});

//emits
const emit = defineEmits(['newLocation']);

//data
const { t } = useI18n();
const router = useRouter();
const q = useQuasar();
let regions: Array<Region> = [];

//Template refs
const fileInputLocationsRef = ref<HTMLInputElement | undefined>(undefined);

//state
let region = ref<Region | undefined>(undefined);
let filteredLocations = ref<Array<Location>>([]);
let selectedLocations = ref<Array<Location>>([]);
let dataLoaded = ref(false);
let addLocationManualDialog = ref(false);
let addLocationFromMapDialog = ref(false);

//computed
const columns = computed(() => {
  return [
    {
      name: 'name',
      required: true,
      label: t(`${props.locationType.toLowerCase()}Name`),
      align: 'left',
      field: (row: Location) => row[t('labelColumn')],
      sortable: true,
    },
    {
      name: 'country',
      label: t('country'),
      align: 'left',
      field: (row: City) => row.country[t('labelColumn')],
      sortable: true,
    },
    {
      name: 'latitude',
      label: t('latitude'),
      align: 'left',
      field: (row: City) => row.latitude,
      sortable: true,
    },
    {
      name: 'longitude',
      label: t('longitude'),
      align: 'left',
      field: (row: City) => row.longitude,
      sortable: true,
    },
    {
      name: 'countryCode',
      label: t('countryCode'),
      align: 'left',
      field: (row: Country) => row.countryCode,
      sortable: true,
    },
  ];
});

const visibleColumns = computed(() => {
  switch (props.locationType) {
    case 'City':
      return ['country', 'latitude', 'longitude'];
    case 'Country':
      return ['countryCode'];
    default:
      return [];
  }
});

//watch
watch(
  () => props.locations,
  () => {
    filterByRegion();
  }
);

//methods
async function getRegions() {
  regions = await retrieveData('region', t('retrieveError'), q, router);
}

function filterByRegion() {
  if (region.value) {
    filteredLocations.value = props.locations?.filter((e) => e.regions.map((r) => r.regionId).includes(region.value.id));
  } else {
    filteredLocations.value = props.locations;
  }
}

function openAddLocationManual() {
  addLocationManualDialog.value = true;
}

function closeAddLocationManual() {
  addLocationManualDialog.value = false;
}

function openAddLocationFromMap() {
  addLocationFromMapDialog.value = true;
}

function closeAddLocationFromMap() {
  addLocationFromMapDialog.value = false;
}

function handleNewLocationFromMap() {
  closeAddLocationFromMap();
  handleNewLocation();
}

function handleNewLocation() {
  closeAddLocationManual();
  emit('newLocation');
}

function exportLocations() {
  let locationsForExport = [];
  if (props.locationType == APP_CONSTANTS.LOCATION_CITY) {
    locationsForExport = selectedLocations.value.map((x) => {
      let city: any = JSON.parse(JSON.stringify(x));
      city.country = { id: (x as City).country.id };
      return city;
    });
  } else if (props.locationType == APP_CONSTANTS.LOCATION_COUNTRY) {
    locationsForExport = selectedLocations.value;
  }
  exportJSON(locationsForExport, `${props.locationType.toLowerCase()}-export`);
  showNotification(t(`${props.locationType.toLowerCase()}ExportSuccessMessage`), MessageType.Success, q);
}

function openLocationsFileInput() {
  fileInputLocationsRef.value?.click();
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files ? target.files[0] : undefined;

  if (file?.type === APP_CONSTANTS.FILE_JSON) {
    const reader = new FileReader();

    reader.onload = async (e: ProgressEvent<FileReader>) => {
      if (e.target) {
        try {
          const jsonObject = JSON.parse(e.target.result as string);
          let locationsCount = await insertData(`${props.locationType}/import`, jsonObject, t(`${props.locationType.toLowerCase()}ImportError`), q, router);
          if (locationsCount != null) {
            if (locationsCount === 0) {
              showNotification(t(`${props.locationType.toLowerCase()}EmptyImportMessage`), MessageType.Success, q);
            } else {
              showNotification(`${locationsCount} ${t(props.locationType.toLowerCase() + 'ImportSuccessMessage')}`, MessageType.Success, q);
              emit('newLocation');
            }
          }
        } catch (error) {
          showNotification(t('unreadableJsonError'), MessageType.Error, q);
        }
      }
    };

    reader.onerror = () => {
      showNotification(t('fileReadError'), MessageType.Error, q);
    };

    reader.readAsText(file);
  } else {
    showNotification(t('fileNotJSSONError'), MessageType.Error, q);
  }
}

//lifecycle hooks
onMounted(async () => {
  await getRegions();
  filteredLocations.value = props.locations;
  dataLoaded.value = true;
});
</script>

<style>
.wide-table {
  width: 100%;
  max-height: 80vh;
}
</style>
