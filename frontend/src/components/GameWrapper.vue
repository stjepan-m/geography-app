<template>
  <div v-if="!gamePrepared && !gameStarted && dataLoaded" class="column shadow-10 rounded-borders">
    <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-primary">
      <p class="text-h6 text-white text-weight-medium no-margin text-uppercase">
        <b>{{ t('newGame') }}</b>
      </p>
    </div>
    <q-tabs v-model="tab" class="text-primary">
      <q-tab :label="t('practice')" name="practice" />
      <q-tab :label="t('enterId')" name="enterId" />
    </q-tabs>
    <q-separator />

    <q-tab-panels v-model="tab" animated>
      <q-tab-panel name="practice" class="column items-center justify-evenly rounded-borders bg-grey-1">
        <q-select filled v-model="gameType" :options="gameTypes" :option-label="labelColumn" :label="t('selectGame')" class="row" style="width: 300px" />
        <q-select
          filled
          :disable="!gameType"
          v-model="region"
          :options="regions"
          :option-label="labelColumn"
          :label="t('selectRegion')"
          class="row q-mt-sm q-mb-md"
          style="width: 300px"
        />
        {{ t('numberOfRounds') }}
        <q-slider :disable="!gameType" v-model="numberOfRounds" label :min="1" :max="20" />
        <q-btn
          rounded
          :disable="!gameType || !region || !numberOfRounds"
          :color="APP_CONSTANTS.COLOR_PRIMARY"
          :label="t('startGame')"
          @click="startPractice"
          class="row q-mt-md text-bold"
        />
      </q-tab-panel>
      <q-tab-panel name="enterId" class="column items-center">
        <q-input filled v-model="gameId" :label="t('gameId5Characters')" class="row" style="width: 300px" />
        <q-btn rounded :disable="!gameId || gameId?.length !== 5" :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t('findGame')" @click="initData()" class="row q-mt-md text-bold" />
      </q-tab-panel>
    </q-tab-panels>
  </div>
  <div v-if="gamePrepared && !gameStarted" class="width-40p">
    <q-stepper v-model="step" :color="APP_CONSTANTS.COLOR_PRIMARY" animated header-class="text-bold text-uppercase">
      <q-step :name="1" :title="t('startGame')" active-icon="map">
        <game-overview
          :gameId="game ? game.id : undefined"
          :gameType="gameType ? gameType[labelColumn] as string : undefined"
          :region="region ? region[labelColumn] as string : undefined"
          :gameName="game ? game.name : undefined"
          :numberOfRounds="game ? game?.numberOfRounds as number : undefined"
          :timeLimitType="timeLimitType ? timeLimitType[labelColumn] as string : undefined"
          :defaultTimeLimitType="defaultTimeLimitType ? defaultTimeLimitType[labelColumn] as string : undefined"
          :timeLimit="game ? game.timeLimitSeconds : undefined"
          :scoringType="scoringType ? scoringType[labelColumn] as string : undefined"
          :allowSkip="game?.allowSkip"
          :allowRetry="game?.allowRetry"
        />
        <q-separator class="q-mt-md q-mb-md" />
        <div class="column items-center">
          <q-btn rounded :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t('play')" class="row q-mt-md text-bold" @click="startGame" />
        </div>
        <q-dialog v-model="gameInProgressDialog" persistent>
          <q-card>
            <q-card-section class="row items-center">
              <span class="q-ml-sm">{{ t('gameInProgressMessage') }}</span>
            </q-card-section>

            <q-card-actions align="right">
              <q-btn flat :label="t('restartGame')" :color="APP_CONSTANTS.COLOR_NEGATIVE" v-close-popup @click="restartGame" />
              <q-btn flat :label="t('continueGame')" :color="APP_CONSTANTS.COLOR_PRIMARY" v-close-popup @click="continueGame" />
            </q-card-actions>
          </q-card>
        </q-dialog>
      </q-step>
    </q-stepper>
  </div>
  <div v-if="gameStarted" class="grow-95 row items-strech justify-evenly bg-grey-1 rounded-borders shadow-10">
    <div class="column relative-position" :class="mapClass">
      <game-map
        v-if="gameStarted"
        ref="mapRef"
        :view="region?.name"
        :zoom="region?.startZoom"
        :start-latitude="region?.startLatitude"
        :start-longitude="region?.startLongitude"
        :interaction-type="gameType?.interactionType"
        :draw-points="gameType?.interactionType === APP_CONSTANTS.INTERACTION_DRAW && gameType?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT"
        :draw-polygons="gameType?.interactionType === APP_CONSTANTS.INTERACTION_DRAW && gameType?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON"
        :match-type="gameType?.interactionType === APP_CONSTANTS.INTERACTION_MATCH ? gameType.featureType : undefined"
        @map-ready="handleMapReady"
        @new-feature="handleNewFeature"
      />
      <div v-if="!gameEnded" class="absolute-top z-high text-center text-h6 text-weight-regular q-py-sm bg-faded">
        <div>
          <q-knob
            v-if="game?.timeLimitSeconds"
            :model-value="timeLeft"
            :max="game?.timeLimitSeconds"
            :step="1"
            font-size="0.4em"
            :class="`text-${timerTrackColor} text-bold`"
            size="40px"
            :thickness="0.25"
            :color="APP_CONSTANTS.COLOR_WHITE"
            :track-color="timerTrackColor"
            readonly
            reverse
            show-value
          />
          {{ gameType?.interactionType == APP_CONSTANTS.INTERACTION_MATCH ? t(`match${gameType.locationType}`) : t('find') }}:
          <b>{{ gameType?.isSequential ? activeRound.location[labelColumn] : '' }}</b>
          <div class="absolute-center z-high q-mt-xl">
            <q-btn v-if="!gameEnded && pendingConfirmation" rounded :color="APP_CONSTANTS.COLOR_POSITIVE" :label="confirmMessage" @click="confirmSelection" class="" />
            <q-btn
              v-if="!gameEnded && !isFinalRound && gameType?.isSequential && game?.allowSkip"
              rounded
              :color="APP_CONSTANTS.COLOR_SECONDARY"
              :label="t('skipRound')"
              @click="skipCurrentRound"
              class="q-ml-sm"
            />
          </div>
        </div>
      </div>
    </div>
    <div v-if="!gameEnded" class="column items-strech justify-start" :class="drawerClass">
      <div class="row rounded-borders no-bottom-radius no-top-left-radius" :class="drawerSubClass">
        <q-btn flat unelevated padding="12px" icon="menu" aria-label="Menu" @click="drawerOpen = !drawerOpen" />
        <p class="text-h6 q-py-sm self-center text-weight-medium text-uppercase no-margin">
          <b v-if="drawerOpen">{{ t('gameDetails') }}</b>
        </p>
      </div>
      <game-details
        v-if="drawerOpen"
        ref="scoreboard"
        :game-set="gameSet"
        :location-type="(gameType?.locationType as string)"
        :current-round="currentRound"
        :max-score="gameType?.interactionType === APP_CONSTANTS.INTERACTION_MATCH ? 1 : 100"
        :show-current-round="(gameType?.isSequential as boolean)"
        :show-previous-rounds="(gameType?.isSequential as boolean)"
        :show-match-list="gameType?.interactionType === APP_CONSTANTS.INTERACTION_MATCH"
        :pending-confirmation="pendingConfirmation"
        class="row grow-1"
        @round-selected="handleNewSelectedRound"
        @selection-confirmed="confirmSelection"
      />
      <q-btn v-if="drawerOpen" rounded :color="APP_CONSTANTS.COLOR_NEGATIVE" :label="t('newGame')" @click="stopGameDialog = true" class="row q-mb-lg self-center" />

      <q-dialog v-model="stopGameDialog" persistent>
        <q-card>
          <q-card-section class="row items-center">
            <span class="q-ml-sm">{{ t('stopGameMessage') }}</span>
          </q-card-section>

          <q-card-actions align="right">
            <q-btn flat :label="t('newGame')" :color="APP_CONSTANTS.COLOR_NEGATIVE" v-close-popup @click="stopGame" />
            <q-btn flat :label="t('continueGame')" :color="APP_CONSTANTS.COLOR_PRIMARY" v-close-popup @click="stopGameDialog = false" />
          </q-card-actions>
        </q-card>
      </q-dialog>
    </div>
    <div v-if="gameEnded" class="column col-3 items-strech justify-evenly">
      <div class="row justify-evenly rounded-borders no-bottom-radius no-top-left-radius q-pa-sm bg-primary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ t('gameDetails') }}</b>
        </p>
      </div>
      <game-details
        ref="scoreboard"
        :game-set="gameSet"
        :location-type="(gameType?.locationType as string)"
        :current-round="currentRound"
        :max-score="gameType?.interactionType === APP_CONSTANTS.INTERACTION_MATCH ? 1 : 100"
        :show-current-round="false"
        :show-previous-rounds="(gameType?.isSequential as boolean)"
        :show-match-list="false"
        :pending-confirmation="false"
        class="row grow-1"
      />
      <q-btn rounded :color="APP_CONSTANTS.COLOR_PRIMARY" :label="t('newGame')" @click="newGame" class="row q-mb-md self-center" />
    </div>
  </div>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter, useRoute } from 'vue-router';
import { useQuasar } from 'quasar';
import { backendBaseURL } from 'boot/api';
import * as turf from '@turf/turf';
import { retrieveData, insertData, updateData } from 'src/utilities/functions';
import GameOverview from './GameOverview.vue';
import GameMap from './GameMap.vue';
import GameDetails from './GameDetails.vue';
import { Region, GameType, TimeLimitType, ScoringType, Game, Round } from 'src/models/models';
import { Point, Polygon, MultiPolygon } from 'ol/geom';
import * as Sphere from 'ol/sphere';
import { useAuthStore } from 'src/stores/authStore';

//emits
const emit = defineEmits(['playerRequested']);

//expose
defineExpose({ setPlayerId });

//data
const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const q = useQuasar();
const authStore = useAuthStore();

const gameId = ref<string | undefined>('game' in route.query ? (route.query.game as string) : undefined);
let gameTypes: Array<GameType> = [];
let regions: Array<Region> = [];
let latestPoint: Point | undefined;
let latestPolygon: MultiPolygon | Polygon | undefined;
let playerId: number;
let timeLimitIntervalId: NodeJS.Timeout;

//state
let mapRef = ref();
let dataLoaded = ref<boolean>(false);
let game = ref<Game | undefined>(undefined);
let gameType = ref<GameType | undefined>(undefined);
let region = ref<Region | undefined>(undefined);
let timeLimitType = ref<TimeLimitType | undefined>(undefined);
let defaultTimeLimitType = ref<TimeLimitType | undefined>(undefined);
let scoringType = ref<ScoringType | undefined>(undefined);
let numberOfRounds = ref(10);
let gamePrepared = ref(false);
let gameStarted = ref(false);
let gameEnded = ref(false);
let isPractice = ref(false);
let tab = ref<string>('practice');
let step = ref<number>(1);
let gameSet = ref<Array<Round>>([]);
let currentRound = ref(0);
let selectedRound = ref<number | undefined>(undefined);
let playerGameId = ref<number | undefined>(undefined);
let suspendedTimeLeft = ref<number | undefined>(undefined);
let gameInProgressDialog = ref(false);
let stopGameDialog = ref(false);
let pendingConfirmation = ref(false);
let drawerOpen = ref(window.innerWidth >= 1024);
const timeLeft = ref(0);

//computed
const labelColumn = computed(() => {
  return t('labelColumn');
});

const activeRound = computed(() => {
  return gameType.value?.isSequential ? gameSet.value[currentRound.value - 1] : gameSet.value[selectedRound.value ? selectedRound.value : 0];
});

const drawerClass = computed(() => {
  return drawerOpen.value ? 'col-3' : 'col-auto';
});

const drawerSubClass = computed(() => {
  return drawerOpen.value ? 'bg-primary text-white' : 'bg-grey-1 text-primary';
});

const mapClass = computed(() => {
  return drawerOpen.value ? 'col-9' : 'col';
});

const confirmMessage = computed(() => {
  return gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH ? t('confirmSelection') : t('confirmLocation');
});

const timerTrackColor = computed(() => {
  if (game.value?.timeLimitSeconds) {
    if (timeLeft.value > 0.5 * game.value.timeLimitSeconds) {
      return APP_CONSTANTS.COLOR_GREEN;
    } else if (timeLeft.value > 0.25 * game.value.timeLimitSeconds) {
      return APP_CONSTANTS.COLOR_ORANGE;
    } else {
      return APP_CONSTANTS.COLOR_RED;
    }
  } else {
    return APP_CONSTANTS.COLOR_GREEN;
  }
});

const isFinalRound = computed(() => {
  return gameSet.value.filter((x) => x.score === null).length === 1;
});

//methods
function setPlayerId(newPlayerId: number) {
  playerId = newPlayerId;
  startGame();
}

function addLocationFromRoundToMap(round: Round) {
  if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
    mapRef.value.addLocationPoint(round.location[labelColumn.value], [round.location.longitude, round.location.latitude], latestPoint);
  } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
    mapRef.value.addLocationPolygon(round.location[labelColumn.value], JSON.parse(round.location.landAndWaterCoordinates as string), latestPolygon);
  }
}

async function calculateScore(round: Round) {
  if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
    let distance = latestPoint ? Number(Sphere.getDistance(latestPoint.getCoordinates(), [round.location.longitude, round.location.latitude]).toFixed(5)) : undefined;
    return await updateData(
      'round/calculateScore',
      {
        round: round,
        distance,
        gameType: gameType.value,
        region: region.value,
        scoringType: scoringType.value,
        timeLimitType: timeLimitType.value,
      },
      t('upsertError'),
      q,
      router
    );
  } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
    let correctArea;
    let missedArea;
    let extraArea;
    if (latestPolygon) {
      let polygon = turf.truncate(turf.multiPolygon(JSON.parse(round.location.landAndWaterCoordinates as string)), { precision: 8 });
      let drawnPolygon = turf.truncate(turf.multiPolygon(latestPolygon.getCoordinates()), { precision: 8 });

      let correct = turf.intersect(polygon, drawnPolygon);
      let missed = turf.difference(polygon, drawnPolygon);
      let extra = turf.difference(drawnPolygon, polygon);

      correctArea = correct !== null ? turf.area(correct) : 0;
      missedArea = missed !== null ? turf.area(missed) : 0;
      extraArea = extra !== null ? turf.area(extra) : 0;
    }

    return await updateData(
      'round/calculateScore',
      {
        round: activeRound.value,
        correctArea,
        missedArea,
        extraArea,
        gameType: gameType.value,
        region: region.value,
        scoringType: scoringType.value,
        timeLimitType: timeLimitType.value,
      },
      t('upsertError'),
      q,
      router
    );
  }
}

async function confirmSelection() {
  pendingConfirmation.value = false;
  let score = await calculateScore(activeRound.value);
  if (score !== undefined) {
    activeRound.value.score = score;
    if (gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
      if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
        mapRef.value.markGuess(score > 0, [activeRound.value.location.longitude, activeRound.value.location.latitude], activeRound.value.location[t('labelColumn')]);
        latestPoint = undefined;
      } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
        mapRef.value.markGuess(score > 0, JSON.parse(activeRound.value.location.landAndWaterCoordinates as string), activeRound.value.location[t('labelColumn')]);
        latestPolygon = undefined;
      }
      selectedRound.value = undefined;

      if (!gameSet.value.filter((x) => x.score === null).length) {
        finishGame();
      }
    } else {
      addLocationFromRoundToMap(activeRound.value);
      latestPoint = undefined;
      latestPolygon = undefined;
      goToNextRound();

      if (!gameEnded.value && timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_PER_ROUND && game.value?.timeLimitSeconds) {
        clearInterval(timeLimitIntervalId);
        generateTimer(game.value?.timeLimitSeconds);
      }
    }
  }
}

async function skipCurrentRound() {
  if (timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_PER_ROUND && game.value?.timeLimitSeconds && timeLeft.value) {
    clearInterval(timeLimitIntervalId);
    activeRound.value.timeLeft = timeLeft.value;
    await updateData(`round/${activeRound.value.id}/setTimeLeft`, timeLeft.value, t('upsertError'), q, router);
  }
  goToNextRound();
}

function goToNextRound() {
  if (!gameSet.value.filter((x) => x.score === null).length) {
    finishGame();
  } else if (game.value?.allowSkip) {
    let nextRound: number | null = null;
    let nextRoundBefore: number | null = null;

    for (let [index, round] of gameSet.value.entries()) {
      if (round.score === null) {
        if (index < currentRound.value - 1 && nextRoundBefore === null) {
          nextRoundBefore = index + 1;
        } else if (index > currentRound.value - 1) {
          nextRound = index + 1;
          break;
        }
      }
    }

    if (nextRound !== null) {
      currentRound.value = nextRound;
    } else if (nextRoundBefore != null) {
      currentRound.value = nextRoundBefore;
    }
  } else {
    currentRound.value++;
  }

  if (!gameEnded.value && timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_PER_ROUND && game.value?.timeLimitSeconds) {
    clearInterval(timeLimitIntervalId);
    generateTimer(activeRound.value.timeLeft ? activeRound.value.timeLeft : game.value?.timeLimitSeconds);
  }
}

async function handleTimeExpiry() {
  if (timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_TOTAL) {
    if (gameType.value?.isSequential) {
      let score = await calculateScore(activeRound.value);
      if (score !== undefined) {
        activeRound.value.score = score;
        addLocationFromRoundToMap(activeRound.value);
        latestPoint = undefined;
        latestPolygon = undefined;
        gameSet.value.slice(currentRound.value).forEach(async (round) => {
          score = await calculateScore(round);

          if (score !== undefined) {
            round.score = score;
            addLocationFromRoundToMap(round);
          }
        });
        currentRound.value = gameSet.value.length + 1;
      }
    } else {
      let score;
      if (selectedRound.value !== undefined) {
        score = await calculateScore(activeRound.value);
        if (score !== undefined) {
          activeRound.value.score = score;
          if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
            mapRef.value.markGuess(score > 0, [activeRound.value.location.longitude, activeRound.value.location.latitude], activeRound.value.location[t('labelColumn')]);
          } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
            mapRef.value.markGuess(score > 0, JSON.parse(activeRound.value.location.landAndWaterCoordinates as string), activeRound.value.location[t('labelColumn')]);
            latestPolygon = undefined;
          }
          selectedRound.value = undefined;
        }
      }

      latestPoint = undefined;
      latestPolygon = undefined;

      gameSet.value.forEach(async (round) => {
        if (round.score === null) {
          score = await calculateScore(round);

          if (score !== undefined) {
            round.score = score;
            if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
              mapRef.value.markGuess(score > 0, [round.location.longitude, round.location.latitude], round.location[t('labelColumn')]);
            } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
              mapRef.value.markGuess(score > 0, JSON.parse(round.location.landAndWaterCoordinates as string), round.location[t('labelColumn')]);
            }
          }
        }
      });
    }
    pendingConfirmation.value = false;
    finishGame();
  } else {
    let score = await calculateScore(activeRound.value);
    if (score !== undefined) {
      activeRound.value.score = score;
      addLocationFromRoundToMap(activeRound.value);
      latestPoint = undefined;
      latestPolygon = undefined;
      pendingConfirmation.value = false;
      goToNextRound();
    }
  }
}

function handleMapReady() {
  if (!gameType.value?.isSequential && gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
    if (gameType.value.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
      mapRef.value.addAllPoints(gameSet.value.map((x) => [x.location.longitude, x.location.latitude]));
    } else if (gameType.value.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
      mapRef.value.addAllPolygons(gameSet.value.map((x) => JSON.parse(x.location.landAndWaterCoordinates as string)));
    }
  }
}

function handleNewFeature(geometry: Point | MultiPolygon) {
  if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POINT) {
    latestPoint = geometry as Point;
  } else if (gameType.value?.featureType === APP_CONSTANTS.FEATURE_TYPE_POLYGON) {
    latestPolygon = geometry as MultiPolygon;
  }
  if (gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_DRAW || selectedRound.value !== undefined) pendingConfirmation.value = true;
}

function handleNewSelectedRound(round: number) {
  selectedRound.value = round;
  if (latestPoint || latestPolygon) pendingConfirmation.value = true;
}

function newGame() {
  gamePrepared.value = false;
  gameStarted.value = false;
  gameEnded.value = false;
  gameSet.value = [];
  gameId.value = undefined;
  playerGameId.value = undefined;
  timeLeft.value = 0;
  latestPoint = undefined;
  latestPolygon = undefined;
  clearInterval(timeLimitIntervalId);
}

async function stopGame() {
  if (playerGameId.value) {
    await cancelGame();
  }
  stopGameDialog.value = false;
  newGame();
}

async function finishGame() {
  mapRef.value.removeInteraction();
  clearInterval(timeLimitIntervalId);
  if (!isPractice.value) await completeGame();
  gameEnded.value = true;
}

function suspendGame() {
  if (playerGameId.value) {
    if (timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_PER_ROUND) {
      if (timeLeft.value) {
        navigator.sendBeacon(backendBaseURL + `/round/${activeRound.value.id}/suspend?timeLeft=${timeLeft.value}`);
      }
      navigator.sendBeacon(backendBaseURL + `/playerGame/${playerGameId.value}/suspend`);
    } else if (timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_TOTAL && timeLeft.value) {
      navigator.sendBeacon(backendBaseURL + `/playerGame/${playerGameId.value}/suspend?timeLeft=${timeLeft.value}`);
    } else {
      navigator.sendBeacon(backendBaseURL + `/playerGame/${playerGameId.value}/suspend`);
    }
  }
}

async function cancelGame() {
  return await updateData(`playerGame/${playerGameId.value}/cancel`, null, t('upsertError'), q, router);
}

async function completeGame() {
  return await updateData(`playerGame/${playerGameId.value}/complete`, null, t('upsertError'), q, router);
}

async function setGameInProgress() {
  return await updateData(`playerGame/${playerGameId.value}/start`, null, t('upsertError'), q, router);
}

async function insertPlayerGame() {
  return await insertData(
    'playerGame',
    {
      gameId: game.value?.id,
      playerId,
    },
    t('upsertError'),
    q,
    router
  );
}

async function getGameSet() {
  let result = await retrieveData(
    isPractice.value
      ? `gameSet?gameTypeId=${gameType.value?.id}&regionId=${region.value?.id}&numberOfRounds=${numberOfRounds.value}`
      : `gameSet?playerGameId=${playerGameId.value}`,
    t('retrieveError'),
    q,
    router
  );

  if (result) {
    gameSet.value = result;
  }
}

function generateTimer(timeLimit: number) {
  timeLeft.value = timeLimit;
  timeLimitIntervalId = setInterval(() => {
    timeLeft.value -= 1;
    if (timeLeft.value <= 0) {
      clearInterval(timeLimitIntervalId);
      handleTimeExpiry();
    }
  }, 1000);
}

async function restartGame() {
  await cancelGame();
  startNewGame();
}

async function continueGame() {
  await getGameSet();

  if (gameSet.value) {
    await setGameInProgress();
    if (gameType.value?.isSequential) {
      currentRound.value = 0;
      goToNextRound();
    }
    if (timeLimitType.value?.name === APP_CONSTANTS.TIME_LIMIT_TYPE_TOTAL && game.value?.timeLimitSeconds) {
      generateTimer(suspendedTimeLeft.value ? suspendedTimeLeft.value : game.value?.timeLimitSeconds);
    }
    if (gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
      drawerOpen.value = true;
    }
    gameStarted.value = true;
  }
}

async function startNewGame() {
  let newPlayerGame = await insertPlayerGame();

  if (newPlayerGame) {
    playerGameId.value = newPlayerGame.id;
    await getGameSet();

    if (gameSet.value) {
      await setGameInProgress();
      if (gameType.value?.isSequential) {
        currentRound.value = 1;
      }
      if (gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
        drawerOpen.value = true;
      }
      if (game.value?.timeLimitSeconds) {
        generateTimer(game.value.timeLimitSeconds);
      }
      gameStarted.value = true;
    }
  }
}

async function startGame() {
  if (authStore.isAuthenticated || playerId) {
    let result = await retrieveData(`playergame/suspended?${playerId ? 'playerId=' + playerId : ''}&gameId=${game.value?.id}`, t('retrieveError'), q, router);

    if (result !== undefined && result.length > 0) {
      playerGameId.value = result[0].id;
      suspendedTimeLeft.value = result[0].timeLeft;
      gameInProgressDialog.value = true;
    } else {
      startNewGame();
    }
  } else {
    emit('playerRequested');
  }
}

async function startPractice() {
  isPractice.value = true;
  await getGameSet();
  if (gameSet.value) {
    if (gameType.value?.isSequential) {
      currentRound.value = 1;
    }
    if (gameType.value?.interactionType === APP_CONSTANTS.INTERACTION_MATCH) {
      drawerOpen.value = true;
    }
    gameStarted.value = true;
  }
}

async function getGame() {
  let result: Game = await retrieveData(`game/${gameId.value}/withRelated`, t('gameIdNotExisting'), q, router);

  if (result) {
    game.value = result as Game;
    gameType.value = result.typeNavigation as GameType;
    region.value = result.regionNavigation as Region;
    timeLimitType.value = result.timeLimitTypeNavigation as TimeLimitType;
    scoringType.value = result.scoringTypeNavigation as ScoringType;
    numberOfRounds.value = result.numberOfRounds;
    defaultTimeLimitType.value = await retrieveData('timeLimitType/default', t('retrieveError'), q, router);

    gamePrepared.value = true;
    dataLoaded.value = true;
  }
}

async function getGameOptions() {
  let gameOptions = await retrieveData('gameOptions', t('retrieveError'), q, router);

  if (gameOptions) {
    gameTypes = gameOptions.gameTypes;
    regions = gameOptions.regions;
    defaultTimeLimitType.value = gameOptions.defaultTimeLimitType;
    dataLoaded.value = true;
  }
}

function initData() {
  if (gameId.value) {
    getGame();
  } else {
    getGameOptions();
  }
}

//lifecycle hooks
onMounted(() => {
  initData();
  window.addEventListener('beforeunload', suspendGame);
});
</script>

<style lang="scss">
.width-40p {
  width: 40%;
}

.grow-1 {
  flex-grow: 1;
}

.grow-95 {
  flex-grow: 0.95;
}

.no-bottom-radius {
  border-bottom-left-radius: 0 !important;
  border-bottom-right-radius: 0 !important;
}

.no-top-left-radius {
  border-top-left-radius: 0 !important;
}

.no-left-radius {
  border-top-left-radius: 0 !important;
  border-bottom-left-radius: 0 !important;
}

.no-right-radius {
  border-top-right-radius: 0 !important;
  border-bottom-right-radius: 0 !important;
}

.border-xs-primary {
  border-style: solid;
  border-color: $primary;
  border-width: 1px;
}

.border-xs-secondary {
  border-style: solid;
  border-color: $secondary;
  border-width: 1px;
}

.bg-faded {
  background-color: rgb(255, 255, 255, 0.8);
}

.z-high {
  z-index: 99;
}
</style>
