<template>
  <div v-if="dataLoaded">
    <div class="q-pa-md">
      <q-stepper v-model="step" class="create-game-wrapper" ref="stepperRef" :color="APP_CONSTANTS.COLOR_PRIMARY" animated>
        <q-step :name="1" :title="t('mainSetupStep')" active-icon="settings" :done="step > 1">
          <div class="column items-center justify-evenly">
            <q-select
              v-model="gameType"
              :options="gameTypes"
              :option-label="labelColumn"
              :label="t('selectGame')"
              class="row width-300"
              :rules="[(val :GameType | undefined) => val !== undefined || t('mandatoryField')]"
              :ref="(r: QSelect) => validationInputsRef[0].push(r)"
            />
            <q-select
              v-model="region"
              :options="regions"
              :option-label="labelColumn"
              :label="t('selectRegion')"
              class="row width-300 q-mt-sm"
              :rules="[(val :Region | undefined) => val !== undefined || t('mandatoryField')]"
              :ref="(r: QSelect) => validationInputsRef[0].push(r)"
            />
            <q-input v-model="gameName" :label="t('gameNameInput')" class="row width-300 q-mt-sm" />
          </div>
        </q-step>
        <q-step :name="2" :title="t('timingAndScoring')" icon="timer" active-icon="timer" :done="step > 2">
          <div class="column items-center justify-evenly">
            <p class="q-field__label q-mb-xs text-weight-bold">
              {{ t('time') }}
            </p>
            <div class="row">
              <q-select
                v-if="gameType !== undefined"
                v-model="timeLimitType"
                :options="timeLimitTypes[gameType.id]"
                :option-label="labelColumn"
                :label="t('timeLimitType')"
                class="column width-300"
                :rules="[(val :TimeLimitType | undefined) => val !== undefined || t('mandatoryField')]"
                @update:model-value="timeLimitInputRef?.resetValidation()"
                :ref="(r: QSelect) => validationInputsRef[1].push(r)"
              />
              <q-input
                ref="timeLimitInputRef"
                :disable="timeLimitType?.id === defaultTimeLimitType?.id"
                type="number"
                v-model.number="timeLimit"
                :label="t('timeLimitSeconds')"
                class="column width-300 q-ml-xl"
                :rules="[(val :TimeLimitType | undefined) => (val !== undefined || timeLimitType?.id === defaultTimeLimitType?.id) || t('mandatoryField')]"
              />
            </div>
            <p class="q-field__label q-mt-lg q-mb-xs text-weight-bold">
              {{ t('scoring') }}
            </p>
            <q-select
              v-if="gameType !== undefined"
              v-model="scoringType"
              :options="scoringTypes[gameType.id]"
              :option-label="labelColumn"
              :label="t('scoringType')"
              class="row width-300"
              :rules="[(val :ScoringType | undefined) => val !== undefined || t('mandatoryField')]"
              :ref="(r: QSelect) => validationInputsRef[1].push(r)"
            />
            <p class="q-field__label q-mt-lg q-mb-xs text-weight-bold">
              {{ t('additionalOptions') }}
            </p>
            <q-checkbox v-model="allowSkip" :label="t('allowSkipRounds')" class="row self-start" />
          </div>
        </q-step>

        <q-step :name="3" :title="t('reviewAndConfirm')" icon="check_circle" active-icon="check_circle" :done="step > 3">
          <game-overview
            :gameType="gameType ? gameType[labelColumn] as string : gameType"
            :region="region ? region[labelColumn] as string : region"
            :gameName="gameName"
            :timeLimitType="timeLimitType ? timeLimitType[labelColumn] as string : timeLimitType"
            :defaultTimeLimitType="defaultTimeLimitType ? defaultTimeLimitType[labelColumn] as string : defaultTimeLimitType"
            :timeLimit="timeLimit"
            :scoringType="scoringType ? scoringType[labelColumn] as string : scoringType"
            :allowSkip="allowSkip"
            :allowRetry="allowRetry"
          />
          <div class="row justify-start">
            <div class="column items-start">
              <p class="q-field__label q-mb-xs q-mt-lg text-weight-bold">
                {{ locationSelectionLabel }}
              </p>
              <q-select v-model="locationSelection" :options="locationSelectionOptions" class="width-300" map-options emit-value />
              <div v-if="locationSelection === APP_CONSTANTS.LOCATION_SELECTION_RANDOM">
                <p class="q-field__label q-mt-lg q-mb-xs self-center">
                  {{ t('numberOfRounds') }}
                </p>
                <q-slider v-model="numberOfRounds" class="width-300" label :min="1" :max="20" />
              </div>
            </div>
          </div>
          <div class="row justify-start">
            <div class="column items-start">
              <p v-if="locationSelection === APP_CONSTANTS.LOCATION_SELECTION_SELECT" class="text-secondary q-mb-xs q-mt-lg">
                <i class="text-secondary q-mr-sm"> <q-icon name="info" /> </i>{{ locationSelectionMessage }}
              </p>
            </div>
          </div>
        </q-step>

        <q-step
          :name="4"
          :title="locationSelection === APP_CONSTANTS.LOCATION_SELECTION_SELECT ? locationSelectionLabel : ''"
          icon="map"
          active-icon="map"
          :disable="locationSelection !== APP_CONSTANTS.LOCATION_SELECTION_SELECT"
          :done="step > 4"
        >
          <div class="row justify-center">
            <location-selection
              ref="locationSelectionRef"
              :locationType="gameType?.locationType"
              :regionId="region?.id"
              :selectedLocations="selectedLocations"
              @new-value="handleNewSelectedLocations"
            />
            <q-btn
              outline
              :color="APP_CONSTANTS.COLOR_PRIMARY"
              @click="openLocationsFileInput"
              icon-right="unarchive"
              :label="t(`${gameType?.locationType}Import`)"
              class="column"
            />
            <input id="locationsImport" type="file" ref="fileInputLocationsRef" accept=".json" @change="handleFileUpload" style="display: none" />
          </div>
        </q-step>

        <q-step :name="5" title="" icon="check" active-icon="check" :done="step > 4">
          <p class="row justify-center q-field__label q-mb-xs text-weight-bold">{{ t('gameCreatedMessage') }} ID: {{ gameId }}</p>
          <div class="q-pa-md row justify-center">
            <q-btn :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t('openGameTracker')" class="column text-bold" :to="`/game?id=${gameId}`" />
            <q-btn flat :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t('newGame')" class="column q-ml-sm text-bold" @click="resetAll" />
          </div>
        </q-step>

        <template v-slot:navigation>
          <q-stepper-navigation>
            <div class="row justify-end">
              <q-btn v-if="step > 1 && step < 5" flat :color="APP_CONSTANTS.COLOR_PRIMARY" @click="stepperRef?.previous()" :label="t('back')" class="column" />
              <q-btn
                v-if="step === 1 && !cloneId"
                outline
                :color="APP_CONSTANTS.COLOR_PRIMARY"
                @click="openGameFileInput"
                icon-right="unarchive"
                :label="t('importGame')"
                class="column"
              />
              <input id="gameImport" type="file" ref="fileInputGameRef" accept=".json" @change="handleFileUpload" style="display: none" />
              <q-btn
                v-if="step < 5"
                @click="if (validateStep(step)) isFinalStep ? saveGame() : stepperRef?.next();"
                :color="APP_CONSTANTS.COLOR_PRIMARY"
                :label="step >= 3 ? t('confirm') : t('next')"
                class="column q-ml-sm"
              />
            </div>
          </q-stepper-navigation>
        </template>
      </q-stepper>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref, onMounted, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { QStepper, QInput, QSelect, useQuasar } from 'quasar';
import { retrieveData, insertData, showNotification } from 'src/utilities/functions';
import LocationSelection from './LocationSelection.vue';
import GameOverview from './GameOverview.vue';
import { GameType, Region, TimeLimitType, ScoringType, Location, TimeLimitTypesByGameTypeId, ScoringTypesByGameTypeId, MessageType, Game } from 'src/models/models';

//data
const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const q = useQuasar();
const cloneId: string | undefined = 'clone' in route.query ? (route.query.clone as string) : undefined;

let gameTypes: Array<GameType> = [];
let regions: Array<Region> = [];
let timeLimitTypes: TimeLimitTypesByGameTypeId = {};

//Template refs
const stepperRef = ref<QStepper | undefined>(undefined);
const locationSelectionRef = ref<typeof LocationSelection | undefined>(undefined);
const timeLimitInputRef = ref<QInput | undefined>(undefined);
const validationInputsRef = ref<Array<Array<QInput | QSelect>>>([[], [], [], []]);
const fileInputGameRef = ref<HTMLInputElement | undefined>(undefined);
const fileInputLocationsRef = ref<HTMLInputElement | undefined>(undefined);

//state
let step = ref<number>(1);
let dataLoaded = ref<boolean>(false);
let gameName = ref<string>('');
let gameType = ref<GameType | undefined>(undefined);
let region = ref<Region | undefined>(undefined);
let numberOfRounds = ref<number>(10);
let timeLimitType = ref<TimeLimitType | undefined>(undefined);
let timeLimit = ref<number | undefined>(undefined);
let scoringTypes = ref<ScoringTypesByGameTypeId>({});
let scoringType = ref<ScoringType | undefined>(undefined);
let locationSelection = ref<string>(APP_CONSTANTS.LOCATION_SELECTION_RANDOM);
let selectedLocations = ref<Array<Location>>([]);
let gameId = ref<string | undefined>(undefined);
let defaultTimeLimitType = ref<TimeLimitType | undefined>(undefined);
let allowSkip = ref<boolean>(false);
let allowRetry = ref<boolean>(false);

//computed
const labelColumn = computed(() => {
  return t('labelColumn');
});

const isFinalStep = computed(() => {
  return step.value === 4 || (step.value === 3 && locationSelection.value === APP_CONSTANTS.LOCATION_SELECTION_RANDOM);
});

const locationSelectionLabel = computed(() => {
  return t(`${gameType.value?.locationType}Selection`);
});

const locationSelectionOptions = computed(() => {
  return [
    { label: t(`${gameType.value?.locationType}RegionRandom`), value: APP_CONSTANTS.LOCATION_SELECTION_RANDOM },
    { label: t(`${gameType.value?.locationType}Select`), value: APP_CONSTANTS.LOCATION_SELECTION_SELECT },
  ];
});

const locationSelectionMessage = computed(() => {
  return t(`${gameType.value?.locationType}SelectionMessage`);
});

//watchers
watch(gameType, (newValue, oldValue) => {
  if (newValue !== undefined) {
    if (timeLimitType.value != undefined && !timeLimitTypes[newValue.id].map((x) => x.id).includes(timeLimitType.value.id)) {
      timeLimitType.value = undefined;
      timeLimit.value = undefined;
    }

    if (scoringType.value != undefined && !scoringTypes.value[newValue.id].map((x) => x.id).includes(scoringType.value.id)) {
      scoringType.value = undefined;
    }

    if (oldValue?.locationType && newValue.locationType !== oldValue?.locationType) {
      selectedLocations.value = [];
    }
  } else {
    timeLimitType.value = undefined;
    timeLimit.value = undefined;
    scoringType.value = undefined;
  }
});

watch(region, (newValue, oldValue) => {
  if (newValue !== undefined && oldValue !== undefined && newValue.id !== oldValue.id) {
    selectedLocations.value = [];
  }
});

watch(timeLimitType, (newValue) => {
  if (newValue?.id === defaultTimeLimitType.value?.id) {
    timeLimit.value = undefined;
  }
});

//methods
async function initData() {
  let gameOptions = await retrieveData('gameOptions', t('retrieveError'), q, router);
  if (gameOptions) {
    gameTypes = gameOptions.gameTypes;
    regions = gameOptions.regions;
    scoringTypes.value = gameOptions.scoringTypes;
    timeLimitTypes = gameOptions.timeLimitTypes;
    defaultTimeLimitType.value = gameOptions.defaultTimeLimitType;

    if (cloneId) {
      let cloneGame = await retrieveData(`game/${cloneId}/info`, t('gameIdNotExisting'), q, router);
      if (cloneGame) {
        let cloneLocations = await retrieveData(`${cloneGame.typeNavigation?.locationType}?game=${cloneId}`, t('retrieveError'), q, router);
        if (cloneLocations) {
          prepopulateData(cloneGame, cloneLocations);
        }
      }
    }
    dataLoaded.value = true;
  }
}

function prepopulateData(game: Game, locations: Array<Location>) {
  gameType.value = game.typeNavigation;
  region.value = game.regionNavigation;
  numberOfRounds.value = game.numberOfRounds;
  timeLimitType.value = game.timeLimitTypeNavigation;
  timeLimit.value = game.timeLimitSeconds;
  scoringType.value = game.scoringTypeNavigation;
  allowSkip.value = game.allowSkip;
  allowRetry.value = game.allowRetry;
  locationSelection.value = game.numberOfRounds === locations.length ? APP_CONSTANTS.LOCATION_SELECTION_SELECT : APP_CONSTANTS.LOCATION_SELECTION_RANDOM;
  selectedLocations.value = game.numberOfRounds === locations.length ? locations : [];
}

function getGameToInsert(numberOfRounds: number) {
  //Generate Game Instance Id
  let inputs = 'abcdefghijklmnopqrstuvwxyz0123456789';
  gameId.value = Array.from(crypto.getRandomValues(new Uint32Array(5)))
    .map((x) => inputs[x % inputs.length])
    .join('');
  return {
    id: gameId.value,
    name: gameName.value,
    type: gameType.value?.id,
    scoringType: scoringType.value?.id,
    region: region.value?.id,
    timeLimitType: timeLimitType.value?.id,
    timeLimitSeconds: timeLimit.value,
    allowSkip: allowSkip.value,
    allowRetry: allowRetry.value,
    numberOfRounds,
  };
}

async function insertGameWithRegionLocations() {
  return await insertData('game/withRegionLocations', getGameToInsert(numberOfRounds.value), t('upsertError'), q, router);
}

async function insertGameWithLocations() {
  const locationIds = selectedLocations.value.map((x) => x.id);
  return await insertData(
    'game/withLocations',
    {
      game: getGameToInsert(locationIds.length),
      locationIds,
    },
    t('upsertError'),
    q,
    router
  );
}

async function saveGame() {
  let result;

  if (locationSelection.value === APP_CONSTANTS.LOCATION_SELECTION_RANDOM) {
    result = await insertGameWithRegionLocations();
  } else {
    result = await insertGameWithLocations();
  }

  if (result) {
    showNotification(t('gameCreatedMessage'), MessageType.Success, q);
    stepperRef.value?.next();
  }
}

function resetAll() {
  step.value = 1;
  gameName.value = '';
  gameType.value = undefined;
  region.value = undefined;
  numberOfRounds.value = 10;
  timeLimitType.value = defaultTimeLimitType.value;
  timeLimit.value = undefined;
  scoringType.value = undefined;
  locationSelection.value = APP_CONSTANTS.LOCATION_SELECTION_RANDOM;
  gameId.value = undefined;
}

function validateStep(step: number) {
  let valid = true;
  validationInputsRef.value[step - 1].forEach((input) => {
    if (input !== null && !input.validate()) valid = false;
  });

  //Manual validation for manually referenced components
  if (step === 2) {
    if (timeLimitInputRef.value && !timeLimitInputRef.value.validate()) valid = false;
  } else if (step === 4) {
    if (locationSelectionRef.value && !locationSelectionRef.value.validate()) {
      valid = false;
    }
  }
  return valid;
}

function handleNewSelectedLocations(locations: Array<Location>) {
  selectedLocations.value = locations;
}

function openGameFileInput() {
  fileInputGameRef.value?.click();
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
          if (target.id === 'gameImport') {
            let locations: Array<Location> = [];
            if (jsonObject.locations) {
              locations = await retrieveData(`${jsonObject.game.typeNavigation?.locationType}?ids=${jsonObject.locations}`, t('retrieveError'), q, router);
              if (locations) {
                prepopulateData(jsonObject.game, locations);
              }
            }
            showNotification(t('gameImportSuccessMessage'), MessageType.Success, q);
          } else {
            let filteredLocations = (jsonObject as Array<Location>).filter((location: Location) => {
              return gameType.value?.locationType === location.type && location.regions.map((x) => x.regionId).includes(region.value?.id as number);
            });
            if (filteredLocations.length) {
              locationSelectionRef.value?.setSelectedLocations(filteredLocations);
              showNotification(`${filteredLocations.length} ${t(gameType.value?.locationType + 'ImportSuccessMessage')}`, MessageType.Success, q);
            } else {
              showNotification(t(`${gameType.value?.locationType}ImportGameError`), MessageType.Error, q);
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
onMounted(() => {
  initData();
});
</script>

<style lang="scss">
.width-300 {
  width: 300px;
}

.create-game-wrapper {
  max-width: 900px;
}
</style>
