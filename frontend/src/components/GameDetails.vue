<template>
  <div class="column items-strech justify-start q-pa-md">
    <div v-if="showCurrentRound && currentRound <= props.gameSet.length" class="column shadow-10 rounded-borders q-mb-lg">
      <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-primary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ roundInfo }}</b>
        </p>
      </div>
      <div class="column items-center justify-evenly q-pa-md rounded-borders bg-grey-1">
        <p class="text-h6 text-black text-weight-regular no-margin">
          {{ activeRound.location[labelColumn] }}
        </p>
      </div>
    </div>
    <div v-if="!props.gameSet.filter((x) => x.score === null).length" class="column shadow-10 rounded-borders q-mb-lg">
      <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-secondary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ t('gameFinished') }}</b>
        </p>
      </div>
    </div>
    <div v-if="showMatchList" class="column shadow-10 rounded-borders q-mb-lg">
      <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-primary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ t(`${locationType}Available`) }}</b>
        </p>
      </div>
      <div class="row items-center justify-center q-pa-md rounded-borders bg-grey-1">
        <q-btn
          v-for="(round, i) in gameSet"
          :key="i"
          :label="(round.location[labelColumn] as string)"
          :disable="round.score !== null"
          :color="round.score === null ? APP_CONSTANTS.COLOR_PRIMARY : round.score > 0 ? APP_CONSTANTS.COLOR_POSITIVE : APP_CONSTANTS.COLOR_NEGATIVE"
          :outline="i !== selectedRound && round.score === null"
          class="q-mx-sm q-my-xs text-bold"
          @click="selectRound(i)"
        />
      </div>
    </div>
    <div v-if="showMatchList" class="column shadow-10 rounded-borders q-mb-lg">
      <div class="row items-center justify-center q-pa-md rounded-borders bg-grey-1">
        <p class="text-secondary text-bold">{{ t(`${locationType}MatchHelpMessage`) }}</p>
        <q-btn
          rounded
          :color="pendingConfirmation ? APP_CONSTANTS.COLOR_POSITIVE : APP_CONSTANTS.COLOR_SECONDARY"
          :disabled="!pendingConfirmation"
          :label="t('confirmSelection')"
          @click="confirmSelection"
        />
      </div>
    </div>
    <div class="column shadow-10 rounded-borders q-mb-lg">
      <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-primary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ t('totalScore') }}</b>
        </p>
      </div>
      <div class="column items-center justify-evenly q-pa-md rounded-borders bg-grey-1">
        <p class="text-h6 text-black text-weight-regular no-margin">
          {{ totalScoreInfo }}
        </p>
      </div>
    </div>
    <div v-if="showPreviousRounds" class="column shadow-10 rounded-borders q-mb-lg overflow-auto grow-1">
      <div class="row items-center justify-evenly rounded-borders no-bottom-radius q-pa-sm bg-primary">
        <p class="text-h6 text-white text-weight-medium text-uppercase no-margin">
          <b>{{ t('previousRounds') }}</b>
        </p>
      </div>
      <q-scroll-area class="row grow-1">
        <div v-for="(round, i) in gameSet.filter((x) => x.score !== null).reverse()" :key="i" class="column items-left justify-evenly q-pa-md rounded-borders bg-grey-1">
          <p class="text-h6 text-black text-weight-regular no-margin">
            <q-knob
              readonly
              :model-value="round.score ? round.score : 0"
              show-value
              size="40px"
              :thickness="0.25"
              :color="round.score > 70 ? APP_CONSTANTS.COLOR_GREEN : round.score > 30 ? APP_CONSTANTS.COLOR_ORANGE : APP_CONSTANTS.COLOR_RED"
              :track-color="round.score > 70 ? APP_CONSTANTS.COLOR_GREEN_FADED : round.score > 30 ? APP_CONSTANTS.COLOR_ORANGE_FADED : APP_CONSTANTS.COLOR_RED_FADED"
              class="q-ml-md q-mr-xs text-body1"
              :class="'text-' + (round.score > 70 ? APP_CONSTANTS.COLOR_GREEN : round.score > 30 ? APP_CONSTANTS.COLOR_ORANGE : APP_CONSTANTS.COLOR_RED)"
            />
            {{ round.location[labelColumn] }}
          </p>
        </div>
      </q-scroll-area>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { ref, computed } from 'vue';
import { Round } from 'src/models/models';
import { useI18n } from 'vue-i18n';

//props
const props = defineProps<{
  locationType: string;
  gameSet: Array<Round>;
  currentRound: number;
  maxScore: number;
  showCurrentRound: boolean;
  showPreviousRounds: boolean;
  showMatchList: boolean;
  pendingConfirmation: boolean;
}>();

//emits
const emit = defineEmits(['roundSelected', 'selectionConfirmed']);

//data
const { t } = useI18n();

//state
let selectedRound = ref<number | undefined>(undefined);

//computed
const labelColumn = computed(() => {
  return t('labelColumn');
});

const roundInfo = computed(() => {
  return `${t('round')} ${props.currentRound} / ${props.gameSet.length}`;
});

const activeRound = computed(() => {
  return props.gameSet[props.currentRound - 1];
});

const totalScoreInfo = computed(() => {
  return `${props.gameSet.reduce((x, y) => x + (y.score ? y.score : 0), 0)} / ${props.maxScore * props.gameSet.filter((x) => x.score !== null).length}`;
});

//methods
function selectRound(round: number) {
  selectedRound.value = round;
  emit('roundSelected', round);
}

function confirmSelection() {
  emit('selectionConfirmed');
}
</script>

<style></style>
