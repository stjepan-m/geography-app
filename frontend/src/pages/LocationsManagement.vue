<template>
  <q-page>
    <q-card class="column">
      <q-tabs v-model="tab" class="text-primary">
        <q-tab :label="t('cities')" name="cities" />
        <q-tab :label="t('countries')" name="countries" />
      </q-tabs>

      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="cities" class="column items-center">
          <locations-table v-if="dataLoaded" locationType="City" :locations="cities" :all-countries="countries" @new-location="getCities" />
        </q-tab-panel>
        <q-tab-panel name="countries" class="column items-center">
          <locations-table v-if="dataLoaded" locationType="Country" :locations="countries" @new-location="getCountries" />
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { retrieveData } from 'src/utilities/functions';
import { City, Country } from 'src/models/models';
import LocationsTable from 'src/components/LocationsTable.vue';

//data
const { t } = useI18n();
const router = useRouter();
const q = useQuasar();

//state
let dataLoaded = ref(false);
let tab = ref<string>('cities');
let cities = ref<Array<City>>([]);
let countries = ref<Array<Country>>([]);

//methods
async function loadData() {
  await getCities();
  await getCountries();
  dataLoaded.value = true;
}

async function getCities() {
  cities.value = await getLocations('city');
}

async function getCountries() {
  countries.value = await getLocations('country');
}

async function getLocations(locationType: string) {
  return await retrieveData(`${locationType}?orderBy=${t('labelColumn')}`, t('retrieveError'), q, router);
}

//lifecycle hooks
onMounted(loadData);
</script>
