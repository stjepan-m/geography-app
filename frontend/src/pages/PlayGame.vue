<template>
  <q-page class="flex items-center justify-evenly">
      <game-wrapper @player-requested="emit('playerRequested')" ref="gameWrapperRef" />
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import GameWrapper from 'src/components/GameWrapper.vue';

const emit = defineEmits(['playerRequested']);
const gameWrapperRef = ref();

function setPlayerId(playerId: number) {
  gameWrapperRef.value.setPlayerId(playerId);
}

defineExpose({ setPlayerId });
</script>
