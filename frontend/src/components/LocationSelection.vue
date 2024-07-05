<template>
  <q-select
    ref="selectRef"
    v-model="selectedLocations"
    :options="options"
    :option-label="t('labelColumn')"
    :label="locationSelectionLabel"
    class="row location-selector"
    menu-anchor="bottom left"
    clearable
    multiple
    use-input
    use-chips
    input-debounce="0"
    :rules="[(val :Array<Location>) => (val && val.length !== 0) || t('mandatoryField')]"
    @filter="filterOptions"
    @add="resetFilter"
  />
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref, onMounted, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { retrieveData } from 'src/utilities/functions';
import { Location } from '../models/models';
import { QSelect, useQuasar } from 'quasar';

//props
const props = defineProps({
  locationType: {
    type: String,
    default: APP_CONSTANTS.LOCATION_CITY.toLowerCase(),
    validator(value: string) {
      return [APP_CONSTANTS.LOCATION_CITY.toLowerCase(), APP_CONSTANTS.LOCATION_COUNTRY.toLowerCase()].includes(value);
    },
  },
  regionId: {
    type: Number,
  },
  selectedLocations: {
    type: Array<Location>,
    default: [],
  },
});

//data
const { t } = useI18n();
const router = useRouter();
const q = useQuasar();
let locations: Array<Location> = [];

const emit = defineEmits({
  newValue: (newValue) => {
    if (newValue) {
      return true;
    } else {
      console.warn('Invalid submit event payload!');
      return false;
    }
  },
});

//Template refs
let selectRef = ref<QSelect | undefined>(undefined);

//state
let selectedLocations = ref<Array<Location>>(props.selectedLocations);
let options = ref<Array<Location>>([]);

//computed
const locationSelectionLabel = computed(() => {
  return props.locationType === APP_CONSTANTS.LOCATION_CITY.toLowerCase() ? t('citySelection') : t('countrySelection');
});

//watchers
watch(selectedLocations, (newValue) => {
  emit('newValue', newValue);
});

//methods
async function getAllOptions() {
  locations = await retrieveData(`${props.locationType}?region=${props.regionId}&orderBy=${t('labelColumn')}`, t('retrieveError'), q, router);
  options.value = locations;
}

function filterOptions(value: string, update: (callbackFunction: () => void) => void) {
  update(() => {
    const searchValue = value.toLowerCase();
    options.value = locations.filter((e) => {
      const labelFields = Object.keys(e).filter((f) => f.startsWith('label'));
      let result = false;
      labelFields.forEach((label) => {
        if ((e[label] as string).toLowerCase().indexOf(searchValue) > -1) {
          result = true;
        }
      });
      return result;
    });
  });
}

function resetFilter() {
  selectRef.value?.updateInputValue('');
}

function setSelectedLocations(locations: Array<Location>) {
  selectedLocations.value = locations;
}

function validate() {
  return selectRef.value?.validate();
}

//expose
defineExpose({
  setSelectedLocations,
  validate,
});

//lifecycle hooks
onMounted(() => {
  getAllOptions();
});
</script>
<style lang="scss">
.location-selector {
  width: 90%;
}

.q-menu {
  max-height: 30vh !important;
}
</style>
