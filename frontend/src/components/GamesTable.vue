<template>
  <q-table
    :rows="filteredGames"
    :columns="columns"
    :visible-columns="visibleColumns"
    virtual-scroll
    :pagination="{ rowsPerPage: 0 }"
    :rows-per-page-options="[0]"
    row-key="id"
    class="wide-table q-ma-sm q-mt-lg"
  >
    <template v-slot:top>
      <q-select v-model="selectedView" :options="views" :label="t('gamesList')" class="q-ml-md" style="width: 200px" @update:model-value="filterBySelectedView" />
      <q-space />
      <q-btn :label="t('createNewGame')" icon-right="open_in_new" :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold" to="create-game" />
    </template>
    <template v-slot:body-cell-id="props">
      <q-td :props="props">
        <router-link :to="`game?id=${props.value}`" class="text-primary text-bold">{{ props.value }}</router-link>
      </q-td>
    </template>
    <template v-slot:body-cell-allowSkip="props">
      <q-td :props="props">
        <q-icon :name="props.value ? 'check_box' : 'check_box_outline_blank'" size="1.5em" />
      </q-td>
    </template>
    <template v-slot:body-cell-allowRetry="props">
      <q-td :props="props">
        <q-icon :name="props.value ? 'check_box' : 'check_box_outline_blank'" size="1.5em" />
      </q-td>
    </template>
    <template v-slot:body-cell-isFinished="props">
      <q-td :props="props">
        <q-icon :name="props.value ? 'check_box' : 'check_box_outline_blank'" size="1.5em" />
      </q-td>
    </template>
    <template v-slot:body-cell-playGame="props">
      <q-td :props="props">
        <q-btn :label="t('play')" icon-right="open_in_new" outline :color="APP_CONSTANTS.COLOR_PRIMARY" class="text-bold" :to="`play?game=${props.value}`" />
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { computed, ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { Game } from 'src/models/models';

//props
const props = defineProps({
  games: {
    type: Array<Game>,
    default: [],
  },
});

//data
const { t } = useI18n();
let views = [
  { value: 'All', label: t('allGames') },
  { value: 'InProgress', label: t('gamesInProgress') },
  { value: 'Finished', label: t('gamesFinished') },
];

//state
let selectedView = ref(views[0]);
let filteredGames = ref<Array<Game>>([]);

//computed
const columns = computed(() => {
  return [
    {
      name: 'id',
      required: true,
      label: t('gameId'),
      align: 'left',
      field: (row: Game) => row.id,
      sortable: true,
    },
    {
      name: 'name',
      required: true,
      label: t('gameName'),
      align: 'left',
      field: (row: Game) => (row.name ? row.name : t('gameWithoutName')),
      sortable: true,
      classes: (row: Game) => (row.name ? 'text-bold' : 'text-italic text-secondary'),
    },
    {
      name: 'gameType',
      required: true,
      label: t('gameType'),
      align: 'left',
      field: (row: Game) => (row.typeNavigation ? row.typeNavigation[t('labelColumn')] : ''),
      sortable: true,
    },
    {
      name: 'region',
      required: true,
      label: t('region'),
      align: 'left',
      field: (row: Game) => (row.regionNavigation ? row.regionNavigation[t('labelColumn')] : ''),
      sortable: true,
    },
    {
      name: 'timeLimitType',
      required: true,
      label: t('timeLimitType'),
      align: 'left',
      field: (row: Game) =>
        row.timeLimitTypeNavigation
          ? row.timeLimitTypeNavigation.isDefault
            ? row.timeLimitTypeNavigation[t('labelColumn')]
            : `${row.timeLimitSeconds} ${t('seconds').toLowerCase()} (${(row.timeLimitTypeNavigation[t('labelColumn')] as string).toLowerCase()})`
          : '',
      sortable: true,
    },
    {
      name: 'scoringType',
      required: true,
      label: t('scoringType'),
      align: 'left',
      field: (row: Game) => (row.scoringTypeNavigation ? row.scoringTypeNavigation[t('labelColumn')] : ''),
      sortable: true,
    },
    {
      name: 'numberOfRounds',
      required: true,
      label: t('numberOfRounds'),
      align: 'left',
      field: (row: Game) => row.numberOfRounds,
      sortable: true,
    },
    {
      name: 'allowSkip',
      required: true,
      label: t('skipAllowed'),
      align: 'center',
      field: (row: Game) => row.allowSkip,
      sortable: true,
    },
    {
      name: 'allowRetry',
      required: true,
      label: t('retryAllowed'),
      align: 'center',
      field: (row: Game) => row.allowRetry,
      sortable: true,
    },
    {
      name: 'isFinished',
      label: t('gameFinished'),
      align: 'center',
      field: (row: Game) => row.isFinished,
      sortable: true,
    },
    {
      name: 'playGame',
      required: true,
      label: '',
      align: 'center',
      field: (row: Game) => row.id,
    },
  ];
});

const visibleColumns = computed(() => {
  switch (selectedView.value.value) {
    case 'All':
      return ['isFinished'];
    default:
      return [];
  }
});

//methods

function filterBySelectedView() {
  switch (selectedView.value.value) {
    case 'InProgress':
      filteredGames.value = props.games?.filter((game) => !game.isFinished);
      break;
    case 'Finished':
      filteredGames.value = props.games?.filter((game) => game.isFinished);
      break;
    default:
      filteredGames.value = props.games;
      break;
  }
}

//lifecycle hooks
onMounted(async () => {
  filteredGames.value = props.games;
});
</script>

<style>
.wide-table {
  width: 95%;
  max-height: 85vh;
}
</style>
