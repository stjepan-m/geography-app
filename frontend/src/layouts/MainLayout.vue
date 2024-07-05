<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-tabs>
          <q-route-tab :label="t('play')" to="/play" exact />
          <q-route-tab v-if="authStore.isAdmin" :label="t('allGames')" to="/games" exact />
          <q-route-tab v-if="authStore.isAdmin || authStore.isTeacher" :label="t('myGames')" to="/my-games" exact />
          <q-route-tab v-if="authStore.isAdmin || authStore.isTeacher" :label="t('createGame')" to="/create-game" exact />
          <q-route-tab v-if="authStore.isAdmin || authStore.isTeacher" :label="t('locations')" to="/locations" exact />
          <q-route-tab :label="t('about')" to="/about" exact />
        </q-tabs>

        <q-space />

        <q-select id="chooseLanguage" map-options emit-value borderless v-model="$i18n.locale" :options="locales" class="language-selector">
          <template v-slot:prepend>
            <q-icon name="translate" :color="APP_CONSTANTS.COLOR_WHITE" />
          </template>
        </q-select>

        <div v-if="!router.currentRoute.value.fullPath.includes('/login') && AppSettings.LoginEnabled">
          <q-btn
            v-if="!authStore.isAuthenticated"
            flat
            dense
            no-caps
            icon="account_circle"
            :label="players && players.length ? players[0].nickname : ''"
            aria-label="Switch Player"
            @click="togglePlayerDialog()"
          />
          <q-btn v-if="authStore.isAuthenticated" flat dense no-caps icon="account_circle" :label="authStore.name" aria-label="User Info" @click="toggleUserDialog()" />
        </div>
      </q-toolbar>
    </q-header>

    <q-dialog v-model="showPlayerDialog" position="right" class="player-dialog">
      <q-card style="width: 350px">
        <p class="text-h6 text-center text-weight-bold no-margin text-uppercase bg-primary text-white q-py-sm">
          {{ t('choosePlayer') }}
        </p>
        <q-card-section v-for="player in players" :key="player.id" class="row no-wrap q-pa-sm" :class="players.indexOf(player) !== 0 ? 'card' : ''" @click="selectPlayer(player)">
          <q-item class="full-width">
            <q-item-section avatar>
              <q-avatar
                :color="players.indexOf(player) === 0 ? APP_CONSTANTS.COLOR_PRIMARY : APP_CONSTANTS.COLOR_GREY_1"
                :text-color="players.indexOf(player) === 0 ? APP_CONSTANTS.COLOR_GREY_1 : APP_CONSTANTS.COLOR_PRIMARY"
                :class="players.indexOf(player) !== 0 ? 'avatar-border' : ''"
                >{{ player.nickname.charAt(0) }}</q-avatar
              >
            </q-item-section>

            <q-item-section>
              <q-item-label>{{ player.nickname }}</q-item-label>
              <q-item-label v-if="players.indexOf(player) === 0" caption>{{ t('activePlayer') }}</q-item-label>
            </q-item-section>
            <q-item-section v-if="players.indexOf(player) !== 0" side>
              <q-btn flat round :color="APP_CONSTANTS.COLOR_NEGATIVE" icon="delete_outline" :aria-label="t('removePlayer')" @click.stop="openRemovePlayerDialog(player)" />
            </q-item-section>
          </q-item>
        </q-card-section>
        <q-card-section class="row items-center no-wrap q-pa-sm card" @click="openAddPlayerDialog">
          <q-item>
            <q-item-section avatar>
              <q-avatar :color="APP_CONSTANTS.COLOR_GREY_1" :text-color="APP_CONSTANTS.COLOR_PRIMARY" class="avatar-border">+</q-avatar>
            </q-item-section>

            <q-item-section>
              <q-item-label>{{ t('newPlayer') }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-card-section>
        <q-card-section class="row items-center justify-center no-wrap q-pa-sm">
          <q-item>
            <q-item-section avatar>
              <q-btn :label="t('login')" :color="APP_CONSTANTS.COLOR_PRIMARY" to="/login" />
            </q-item-section>
          </q-item>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showAddPlayerDialog" :persistent="playerRequested">
      <q-card>
        <q-card-section>
          <div class="text-h6 text-center text-uppercase text-bold text-primary">
            {{ t('newPlayer') }}
          </div>
        </q-card-section>
        <q-card-section class="row items-center">
          <q-input v-model="newPlayerName" :label="t('playerName')" style="width: 300px" class="q-px-lg" />
        </q-card-section>

        <q-card-actions align="right">
          <q-btn :disable="!newPlayerName" flat :label="t('addPlayer')" :color="APP_CONSTANTS.COLOR_PRIMARY" v-close-popup class="text-bold" @click="addPlayer" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showRemovePlayerDialog" persistent>
      <q-card>
        <q-card-section class="row items-center">
          <span class="q-ml-sm">{{ t('removePlayerConfirmation') }}</span>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('cancel')" :color="APP_CONSTANTS.COLOR_PRIMARY" v-close-popup @click="resetRemoval" />
          <q-btn flat :label="t('removePlayer')" :color="APP_CONSTANTS.COLOR_NEGATIVE" v-close-popup @click="removePlayer" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showUserDialog" position="right" class="player-dialog">
      <q-card style="width: 350px">
        <p class="text-h6 text-center text-weight-bold no-margin text-uppercase bg-primary text-white q-py-sm">
          {{ t('user') }}
        </p>
        <q-card-section class="row no-wrap q-pa-sm">
          <q-item class="full-width">
            <q-item-section avatar>
              <q-avatar :color="APP_CONSTANTS.COLOR_PRIMARY" text-color="grey-1" class="avatar-border">{{ authStore.name.charAt(0) }}</q-avatar>
            </q-item-section>

            <q-item-section>
              <q-item-label>{{ authStore.name }}</q-item-label>
              <q-item-label caption>{{ authStore.email }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-card-section>
        <q-card-section class="row items-center justify-center no-wrap q-pa-sm">
          <q-item>
            <q-item-section avatar>
              <q-btn :label="t('logout')" :color="APP_CONSTANTS.COLOR_PRIMARY" @click="openLogoutDialog" />
            </q-item-section>
          </q-item>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showLogoutDialog" persistent>
      <q-card>
        <q-card-section class="row items-center">
          <span class="q-ml-sm">{{ t('logoutConfirmation') }}</span>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat :label="t('cancel')" :color="APP_CONSTANTS.COLOR_PRIMARY" v-close-popup @click="closeLogoutDialog" />
          <q-btn flat :label="t('confirm')" :color="APP_CONSTANTS.COLOR_NEGATIVE" v-close-popup @click="handleLogout" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-page-container>
      <router-view @player-requested="handlePlayerRequest" v-slot="{ Component }">
        <component ref="routerRef" :is="Component" />
      </router-view>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import * as APP_CONSTANTS from 'src/utilities/constants';
import { onMounted, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { RouterView, useRouter } from 'vue-router';
import { insertData, showNotification } from 'src/utilities/functions';
import { Player, MessageType } from 'src/models/models';
import { useAuthStore } from 'src/stores/authStore';
import AppSettings from 'src/AppSettings';

const { t } = useI18n();
const router = useRouter();
const q = useQuasar();

let playerRequested = false;
let playerToBeRemoved: Player | undefined = undefined;

const routerRef = ref();
const authStore = useAuthStore();

const locales = ref(APP_CONSTANTS.LANGUAGE_OPTIONS);
let showPlayerDialog = ref(false);
let showAddPlayerDialog = ref(false);
let showRemovePlayerDialog = ref(false);
let showUserDialog = ref(false);
let showLogoutDialog = ref(false);
let players = ref<Array<Player>>([]);
let newPlayerName = ref('');

//methods
function togglePlayerDialog() {
  showPlayerDialog.value = !showPlayerDialog.value;
}

function openAddPlayerDialog() {
  showAddPlayerDialog.value = true;
}

function openRemovePlayerDialog(player: Player) {
  playerToBeRemoved = player;
  showRemovePlayerDialog.value = true;
}

function toggleUserDialog() {
  showUserDialog.value = !showUserDialog.value;
}

function openLogoutDialog() {
  showLogoutDialog.value = true;
}

function closeLogoutDialog() {
  showLogoutDialog.value = false;
}

function selectPlayer(player: Player) {
  showPlayerDialog.value = false;
  players.value.splice(players.value.indexOf(player), 1);
  players.value.unshift(player);
  localStorage.players = JSON.stringify(players.value);
  showNotification(t('playerChangedMessage'), MessageType.Success, q);
}

function removePlayer() {
  players.value.splice(players.value.indexOf(playerToBeRemoved as Player), 1);
  resetRemoval();
  localStorage.players = JSON.stringify(players.value);
  showNotification(t('playerRemovedMessage'), MessageType.Success, q);
}

function resetRemoval() {
  playerToBeRemoved = undefined;
}

function handlePlayerRequest() {
  if (players.value.length > 0) {
    routerRef.value.setPlayerId(players.value[0].id);
  } else {
    playerRequested = true;
    openAddPlayerDialog();
  }
}

async function addPlayer() {
  let player = await insertData('player', { nickname: newPlayerName.value }, t('upsertError'), q, router);
  if (player) {
    let playersTemp = [];
    if (localStorage.players) {
      playersTemp = JSON.parse(localStorage.players);
    }

    playersTemp.unshift(player);
    localStorage.players = JSON.stringify(playersTemp);
    players.value = playersTemp;
    showPlayerDialog.value = false;
    if (playerRequested) {
      playerRequested = false;
      routerRef.value.setPlayerId(players.value[0].id);
    }
    showNotification(t('playerAddedMessage'), MessageType.Success, q);
  }
}

function handleLogout() {
  closeLogoutDialog();
  toggleUserDialog();
  showNotification(t('logoutSuccess'), MessageType.Success, q);
  authStore.handleLogout();
  router.push('/');
}

onMounted(() => {
  if (localStorage.players) players.value = JSON.parse(localStorage.players);
});
</script>

<style lang="scss">
.language-selector .q-field__native,
.language-selector .q-field__append {
  color: white;
}

.player-dialog > .q-dialog__inner--right {
  top: -525px !important;
}

.card:hover {
  background-color: $grey-1;
  cursor: pointer;
}

.avatar-border {
  outline: 2px solid $primary;
}
</style>
