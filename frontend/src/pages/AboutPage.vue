<template>
  <q-page>
    <q-page-container class="q-pa-md column items-center justify-center">
      <div class="text-h5 text-bold q-mb-md">{{ t('appInformation') }}</div>
      <p>{{ t('appPurpose') }}</p>
      <p>
        <strong>{{ t('author') }}:</strong> Stjepan Mlakić (Mentor: prof. dr. sc. Boris Milašinović)
      </p>
    </q-page-container>
  </q-page>
</template>

<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>

<style scoped></style>
