<template>
  <GoogleLogin :callback="callback" prompt auto-login class="fixed-center" />
</template>

<script setup>
import { useI18n } from 'vue-i18n';
import { useQuasar } from 'quasar';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from 'stores/authStore';
import { showNotification } from 'src/utilities/functions';
import { MessageType } from 'src/models/models';

const { t } = useI18n();
const q = useQuasar();
const route = useRoute();
const router = useRouter();

const callback = async (response) => {
  if (response.credential) {
    await useAuthStore().handleLogin(response.credential, t('loginError'), q);
    if (useAuthStore().isAuthenticated) {
      showNotification(t('loginSuccess'), MessageType.Success, q);
      router.push(route.query.redirect ? route.query.redirect : '/');
    }
  }
};
</script>
