<template>
  <q-page class="row justify-center items-center">
    <q-card>
      <q-card-section>
        <div class="text-h6 text-primary text-uppercase">{{ t('accessDenied') }}</div>
      </q-card-section>
      <q-card-section>
        <p>{{ t('noAccessToPage') }}</p>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat :label="t('back')" @click="goBack" class="text-primary" />
      </q-card-actions>
    </q-card>
  </q-page>
</template>

<script setup>
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';

const { t } = useI18n();

const router = useRouter();

function goBack() {
  router.go(-2);
}
</script>
