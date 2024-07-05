<template>
  <div class="row justify-evenly full-width">
    <div class="column col-6 items-start justify-start">
      <q-field v-if="gameId" borderless :label="t('gameId')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ gameId }}
          </div>
        </template>
      </q-field>
      <q-field v-else-if="gameName" borderless :label="t('gameName')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ gameName }}
          </div>
        </template>
      </q-field>
    </div>
    <div class="column col-6 items-start justify-start">
      <q-field v-if="gameId && gameName" borderless :label="t('gameName')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ gameName }}
          </div>
        </template>
      </q-field>
    </div>
  </div>
  <div class="row justify-evenly full-width">
    <div class="column col-6 items-start justify-start">
      <q-field borderless :label="t('gameType')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ gameType }}
          </div>
        </template>
      </q-field>
      <q-field v-if="numberOfRounds" borderless :label="t('numberOfRounds')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ numberOfRounds }}
          </div>
        </template>
      </q-field>
    </div>
    <div class="column col-6 items-start justify-start">
      <q-field borderless :label="t('region')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ region }}
          </div>
        </template>
      </q-field>
    </div>
  </div>
  <div class="row justify-evenly full-width">
    <div class="column col-6 items-start">
      <q-field borderless :label="t('timeLimit')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ timeLimitTypeValue }}
          </div>
        </template>
      </q-field>
    </div>
    <div class="column col-6 items-start">
      <q-field borderless :label="t('scoringType')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ scoringType ? scoringType : '' }}
          </div>
        </template>
      </q-field>
    </div>
  </div>
  <div class="row justify-evenly full-width">
    <div class="column col-6 items-start">
      <q-field borderless :label="t('allowSkipRounds')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ allowSkip ? t('yes') : t('no') }}
          </div>
        </template>
      </q-field>
    </div>
    <div class="column col-6 items-start">
      <q-field borderless :label="t('allowRetryRounds')" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ allowRetry ? t('yes') : t('no') }}
          </div>
        </template>
      </q-field>
    </div>
  </div>

  <div v-if="locationType && locations" class="row justify-evenly full-width">
    <div class="column col-12 items-start">
      <q-field borderless :label="t(`${locationType}`)" stack-label :label-color="APP_CONSTANTS.COLOR_PRIMARY" class="full-width">
        <template v-slot:control>
          <div class="self-center full-width no-outline" tabindex="0">
            {{ locations }}
          </div>
        </template>
      </q-field>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

//props
const props = defineProps({
  gameId: {
    type: String,
  },
  gameType: {
    type: String,
  },
  region: {
    type: String,
  },
  gameName: {
    type: String,
  },
  numberOfRounds: {
    type: Number,
  },
  timeLimitType: {
    type: String,
  },
  defaultTimeLimitType: {
    type: String,
  },
  timeLimit: {
    type: Number,
  },
  scoringType: {
    type: String,
  },
  allowSkip: {
    type: Boolean,
    default: false,
  },
  allowRetry: {
    type: Boolean,
    default: false,
  },
  locationType: {
    type: String,
  },
  locations: {
    type: String,
  },
});

//data
const { t } = useI18n();

//state
let gameId = ref<string | undefined>(props.gameId);
let gameName = ref<string | undefined>(props.gameName);
let gameType = ref<string | undefined>(props.gameType);
let region = ref<string | undefined>(props.region);
let numberOfRounds = ref<number | undefined>(props.numberOfRounds);
let timeLimitType = ref<string | undefined>(props.timeLimitType);
let timeLimit = ref<number | undefined>(props.timeLimit);
let scoringType = ref<string | undefined>(props.scoringType);
let defaultTimeLimitType = ref<string | undefined>(props.defaultTimeLimitType);
let allowSkip = ref<boolean | undefined>(props.allowSkip);
let allowRetry = ref<boolean | undefined>(props.allowRetry);
let locationType = ref<string | undefined>(props.locationType);
let locations = ref<string | undefined>(props.locations);

//computed
const timeLimitTypeValue = computed(() => {
  return timeLimitType?.value
    ? timeLimitType.value === defaultTimeLimitType.value
      ? timeLimitType.value
      : `${timeLimit.value} ${t('seconds').toLowerCase()} (${timeLimitType.value.toString().toLowerCase()})`
    : '';
});
</script>
