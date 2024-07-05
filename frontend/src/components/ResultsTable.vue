<template>
  <q-table
    :rows="results"
    :columns="columns"
    :visible-columns="visibleColumns"
    virtual-scroll
    :pagination="{ rowsPerPage: 0 }"
    :rows-per-page-options="[0]"
    row-key="name"
    class="wide-table q-ma-sm"
  />
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { PlayerGame } from 'src/models/models';

//props
const props = defineProps({
  results: Array<PlayerGame>,
});

//data
const { t } = useI18n();
const columns = [
  {
    name: 'player',
    required: true,
    label: t('playerName'),
    align: 'left',
    field: (row: PlayerGame) => row.player?.nickname,
    sortable: true,
  },
  {
    name: 'status',
    label: t('gameStatus'),
    align: 'left',
    field: (row: PlayerGame) => getStatus(row.status),
    sortable: true,
  },
  {
    name: 'roundsCompleted',
    required: true,
    label: t('roundsCompleted'),
    align: 'left',
    field: (row: PlayerGame) => row.roundsCompleted,
    format: (val: number, row: PlayerGame) => `${val} / ${row.game?.numberOfRounds}`,
    sortable: true,
  },
  {
    name: 'totalScore',
    required: true,
    label: t('totalScore'),
    align: 'left',
    field: (row: PlayerGame) => row.totalScore,
    format: (val: number, row: PlayerGame) => `${val} / ${row.roundsCompleted * (row.game?.scoringTypeNavigation ? row.game.scoringTypeNavigation.maxScore : 100)}`,
    sortable: true,
  },
  {
    name: 'scorePercentage',
    required: true,
    label: t('scorePercentage'),
    align: 'left',
    field: (row: PlayerGame) =>
      row.roundsCompleted ? (row.totalScore / (row.roundsCompleted * (row.game?.scoringTypeNavigation ? row.game.scoringTypeNavigation.maxScore : 100))) * 100 : 0,
    format: (val: number, row: PlayerGame) => (row.roundsCompleted ? `${Math.round(val * 100) / 100}%` : '-'),
    sortable: true,
  },
];

//state

//computed
const visibleColumns = computed(() => {
  return ['status'];
});

//methods
function getStatus(status: string) {
  switch (status) {
    case APP_CONSTANTS.GAME_STATUS_NOT_STARTED:
      return t('gameStatusNotStarted');
    case APP_CONSTANTS.GAME_STATUS_IN_PROGRESS:
      return t('gameStatusInProgress');
    case APP_CONSTANTS.GAME_STATUS_SUSPENDED:
      return t('gameStatusSuspended');
    case APP_CONSTANTS.GAME_STATUS_CANCELLED:
      return t('gameStatusCancelled');
    case APP_CONSTANTS.GAME_STATUS_COMPLETED:
      return t('gameStatusCompleted');
    default:
      return undefined;
  }
}
</script>
<style>
.wide-table {
  width: 100%;
  max-height: 80vh;
}
</style>
