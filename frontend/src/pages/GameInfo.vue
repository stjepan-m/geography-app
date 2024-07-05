<template>
  <q-page>
    <q-card v-if="dataLoaded" class="column">
      <q-tabs v-model="tab" class="text-primary">
        <q-tab :label="t('details')" name="details" />
        <q-tab :label="t('results')" name="results" />
      </q-tabs>

      <q-tab-panels v-model="tab" animated>
        <q-tab-panel name="details" class="column items-center">
          <q-card class="row q-mb-md q-pa-md full-width">
            <game-overview
              :gameId="game ? game.id : undefined"
              :gameType="game?.typeNavigation ? game.typeNavigation[t('labelColumn')] as string : undefined"
              :region="game?.regionNavigation ? game.regionNavigation[t('labelColumn')] as string : undefined"
              :gameName="game ? game.name : undefined"
              :numberOfRounds="game ? game?.numberOfRounds as number : undefined"
              :timeLimitType="game?.timeLimitTypeNavigation ? game.timeLimitTypeNavigation[t('labelColumn')] as string : undefined"
              :defaultTimeLimitType="defaultTimeLimitType ? defaultTimeLimitType[t('labelColumn')] as string : undefined"
              :timeLimit="game ? game.timeLimitSeconds : undefined"
              :scoringType="game?.scoringTypeNavigation ? game.scoringTypeNavigation[t('labelColumn')] as string : undefined"
              :allowSkip="game?.allowSkip"
              :allowRetry="game?.allowRetry"
              :locationType="locationType"
              :locations="locationsString"
            />
          </q-card>
          <div class="row">
            <q-btn :label="t('play')" icon-right="play_circle" :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold" :to="`play?game=${gameId}`">
              <q-tooltip class="text-caption">{{ t('playGameTooltip') }}</q-tooltip>
            </q-btn>
            <q-btn :label="t('cloneGame')" icon-right="file_copy" outline :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold q-ml-sm" :to="`create-game?clone=${gameId}`">
              <q-tooltip class="text-caption">{{ t('cloneGameTooltip') }}</q-tooltip>
            </q-btn>
            <q-btn :label="t('exportGame')" icon-right="archive" outline :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold q-ml-sm" @click="exportGame">
              <q-tooltip class="text-caption">{{ t('exportGameTooltip') }}</q-tooltip>
            </q-btn>
            <q-btn
              v-if="game?.numberOfRounds === gameLocations?.length"
              :label="t(`${game?.typeNavigation?.locationType}Export`)"
              icon-right="archive"
              outline
              :color="APP_CONSTANTS.COLOR_PRIMARY"
              class="text-bold q-ml-sm"
              @click="exportLocations"
            >
              <q-tooltip class="text-caption">{{ t('exportLocationsTooltip') }}</q-tooltip>
            </q-btn>
          </div>
        </q-tab-panel>
        <q-tab-panel name="results" class="column items-center">
          <results-table v-if="results" :results="results" />
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useQuasar } from 'quasar';
import { retrieveData, exportJSON, showNotification } from 'src/utilities/functions';
import { Game, PlayerGame, TimeLimitType, Location, City, MessageType } from 'src/models/models';
import GameOverview from 'src/components/GameOverview.vue';
import ResultsTable from 'src/components/ResultsTable.vue';

//data
const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const q = useQuasar();
const gameId: string | undefined = 'id' in route.query ? (route.query.id as string) : undefined;

//state
let game = ref<Game | undefined>();
let gameLocations = ref<Array<Location>>([]);
let results = ref<Array<PlayerGame>>();
let tab = ref<string>('details');
let defaultTimeLimitType = ref<TimeLimitType | undefined>(undefined);
let dataLoaded = ref<boolean>(false);

//computed
const locationsString = computed(() => {
  if (game.value?.numberOfRounds !== gameLocations.value?.length) {
    return t(`${game.value?.typeNavigation?.locationType}RegionRandom`);
  } else {
    return gameLocations.value?.map((location) => location[t('labelColumn')]).join(', ');
  }
});

//computed
const locationType = computed(() => {
  switch (game.value?.typeNavigation?.locationType) {
    case APP_CONSTANTS.LOCATION_CITY.toLowerCase():
      return 'cities';
    case APP_CONSTANTS.LOCATION_COUNTRY.toLowerCase():
      return 'countries';
    default:
      return 'locations';
  }
});

//methods
async function loadData() {
  if (gameId) {
    let result = await retrieveData(`game/${gameId}/info`, t('gameIdNotExisting'), q, router);
    if (result) {
      game.value = result;
      result = await retrieveData(`playerGame?gameId=${gameId}`, t('retrieveError'), q, router);
      if (result) {
        results.value = result;
        result = await retrieveData(`${game.value?.typeNavigation?.locationType}?game=${gameId}`, t('retrieveError'), q, router);
        if (result) {
          gameLocations.value = result;
          defaultTimeLimitType.value = await retrieveData('timeLimitType/default', t('retrieveError'), q, router);
          dataLoaded.value = true;
        }
      }
    }
  } else {
    showNotification(t('noGameId'), MessageType.Error, q);
  }
}

function exportGame() {
  let exportObject;
  if (game.value?.numberOfRounds === gameLocations.value?.length) {
    exportObject = { game: game.value, locations: gameLocations.value?.map((x) => x.id) };
  } else {
    exportObject = { game: game.value };
  }
  exportJSON(exportObject, game.value ? game.value.id : 'game');
  showNotification(t('gameExportSuccessMessage'), MessageType.Success, q);
}

function exportLocations() {
  let locationsForExport = [];
  if (game.value?.typeNavigation?.locationType == APP_CONSTANTS.LOCATION_CITY.toLowerCase()) {
    locationsForExport = gameLocations.value.map((x) => {
      let city: any = JSON.parse(JSON.stringify(x));
      city.country = { id: (x as City).country.id };
      return city;
    });
  } else if (game.value?.typeNavigation?.locationType == APP_CONSTANTS.LOCATION_COUNTRY.toLowerCase()) {
    locationsForExport = gameLocations.value;
  }
  exportJSON(locationsForExport, `${game.value ? game.value.id : 'game'}-${locationType.value}`);
  showNotification(t(`${game.value?.typeNavigation?.locationType}ExportSuccessMessage`), MessageType.Success, q);
}

//lifecycle hooks
onMounted(loadData);
</script>
<style>
.full-width {
  width: 100%;
}
</style>
