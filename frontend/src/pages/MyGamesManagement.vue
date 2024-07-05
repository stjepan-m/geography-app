<template>
  <q-page class="column items-center">
    <games-table v-if="dataLoaded" :games="games" />
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { retrieveData } from 'src/utilities/functions';
import { Game } from 'src/models/models';
import GamesTable from 'src/components/GamesTable.vue';

//data
const { t } = useI18n();
const router = useRouter();
const q = useQuasar();

//state
let dataLoaded = ref(false);
let games = ref<Array<Game>>([]);

//methods
async function loadData() {
  games.value = await retrieveData('game/myGames/withRelated', t('retrieveError'), q, router);
  if (games.value) {
    dataLoaded.value = true;
  }
}

//lifecycle hooks
onMounted(loadData);
</script>
