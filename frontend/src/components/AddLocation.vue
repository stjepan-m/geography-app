<template>
  <q-card>
    <q-card-section>
      <div class="text-h6 text-center text-uppercase text-bold text-primary">
        {{ t(`add${locationType}`) }}
      </div>
    </q-card-section>
    <q-card-section class="column items-center q-px-xl">
      <q-input
        v-model="nameEn"
        :label="t(`${locationType.toLowerCase()}NameEn`)"
        lazy-rules
        :rules="[(val) => (val && val.length > 0) || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QInput) => inputsRef.push(r)"
      />
      <q-input
        v-model="nameHr"
        :label="t(`${locationType.toLowerCase()}NameHr`)"
        lazy-rules
        :rules="[(val) => (val && val.length > 0) || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QInput) => inputsRef.push(r)"
      />
      <q-input
        v-if="APP_CONSTANTS.POINT_LOCATIONS.includes(locationType)"
        type="number"
        v-model="latitude"
        :label="t('latitude')"
        lazy-rules
        :rules="[(val) => val || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QInput) => inputsRef.push(r)"
      />
      <q-input
        v-if="APP_CONSTANTS.POINT_LOCATIONS.includes(locationType)"
        type="number"
        v-model="longitude"
        :label="t('longitude')"
        lazy-rules
        :rules="[(val) => val || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QInput) => inputsRef.push(r)"
      />
      <q-select
        v-if="locationType === APP_CONSTANTS.LOCATION_CITY"
        v-model="country"
        :options="countries"
        :option-label="t('labelColumn')"
        :label="t('country')"
        lazy-rules
        :rules="[(val) => val || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QSelect) => inputsRef.push(r)"
      />
      <q-input
        v-if="APP_CONSTANTS.POLYGON_LOCATIONS.includes(locationType)"
        v-model="areaCoordinates"
        type="textarea"
        :label="t('areaCoordinates')"
        lazy-rules
        :rules="[(val) => (val && val.length > 0) || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QInput) => inputsRef.push(r)"
      /><q-select
        v-model="selectedRegions"
        :options="regions"
        :option-label="t('labelColumn')"
        :label="t('selectRegion')"
        class="row location-selector"
        clearable
        multiple
        use-input
        input-debounce="0"
        :rules="[(val :Array<Region>) => (val && val.length > 0) || t('mandatoryField')]"
        style="width: 300px"
        :ref="(r: QSelect) => inputsRef.push(r)"
      />
    </q-card-section>

    <q-card-actions align="right">
      <q-btn flat :label="t(`add${locationType}`)" :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold" @click="addLocation" />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { QInput, QSelect, useQuasar } from 'quasar';
import { insertData, showNotification } from 'src/utilities/functions';
import { Location, City, Country, RegionLocation, Region, MessageType } from 'src/models/models';

//data
const { t } = useI18n();
const router = useRouter();
const q = useQuasar();

//refs
const inputsRef = ref<Array<QInput | QSelect>>([]);

//props
const props = defineProps({
  location: {
    type: Object,
  },
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
let nameEn = ref<string | undefined>(props.location?.labelEn);
let nameHr = ref<string | undefined>(props.location?.labelHr);
let latitude = ref<number | undefined>(props.location?.latitude);
let longitude = ref<number | undefined>(props.location?.longitude);
let country = ref<Country | undefined>(props.location?.country);
let selectedRegions = ref<Array<Region>>([]);
let areaCoordinates = ref<string | undefined>(undefined);

//methods
async function addLocation() {
  let isValid = true;
  inputsRef.value.forEach((input) => {
    if (!input.validate()) {
      isValid = false;
    }
  });

  if (isValid) {
    let result: Location;
    let locationToInsert: Location | undefined;
    switch (props.locationType) {
      case APP_CONSTANTS.LOCATION_CITY:
        locationToInsert = {
          name: nameEn.value,
          type: props.locationType.toLowerCase(),
          labelEn: nameEn.value,
          labelHr: nameHr.value,
          latitude: latitude.value,
          longitude: longitude.value,
          country: country.value,
          regions: selectedRegions.value.map((v) => {
            return {
              regionId: v.id,
            } as RegionLocation;
          }),
        } as City;
        break;
      default:
        locationToInsert = undefined;
        break;
    }

    result = await insertData('city', locationToInsert, t('upsertError'), q, router);

    if (result) {
      emit('newLocation', result);
      showNotification(t(`${props.locationType.toLowerCase()}CreatedMessage`), MessageType.Success, q);
    }
  }
}
</script>
<style></style>
