<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { setLocale } from '@/locales';

const { locale, t } = useI18n();

const currentLocale = computed({
  get: () => locale.value,
  set: (value: string) => setLocale(value as 'en' | 'vi'),
});

const locales = [
  { code: 'en', name: 'EN', flag: '🇺🇸' },
  { code: 'vi', name: 'VI', flag: '🇻🇳' },
];
</script>

<template>
  <div class="language-switcher">
    <button
      v-for="lang in locales"
      :key="lang.code"
      :class="['lang-btn', { active: currentLocale === lang.code }]"
      @click="currentLocale = lang.code"
      :title="t(`language.${lang.code}`)"
    >
      <span class="flag">{{ lang.flag }}</span>
      <span class="code">{{ lang.name }}</span>
    </button>
  </div>
</template>

<style scoped>
.language-switcher {
  display: flex;
  gap: 0.25rem;
}

.lang-btn {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 4px;
  color: inherit;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.2s;
}

.lang-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

.lang-btn.active {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.4);
}

.flag {
  font-size: 1rem;
}

.code {
  font-weight: 600;
}
</style>
