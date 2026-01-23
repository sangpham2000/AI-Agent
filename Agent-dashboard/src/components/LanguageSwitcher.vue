<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { setLocale } from '@/locales'
import AppIcon from '@/components/ui/AppIcon.vue'

const { locale } = useI18n()

const currentLocale = computed({
  get: () => locale.value,
  set: (value: string) => setLocale(value as 'en' | 'vi'),
})

const locales = [
  { code: 'en', label: 'English', short: 'EN' },
  { code: 'vi', label: 'Tiếng Việt', short: 'VI' },
]

const activeLocale = computed(
  () => locales.find((l) => l.code === currentLocale.value) || locales[0],
)
</script>

<template>
  <div class="dropdown dropdown-end">
    <button
      tabindex="0"
      class="btn btn-ghost btn-sm btn-square rounded-lg group"
      title="Change Language"
    >
      <AppIcon name="translate" class="w-[18px] h-[18px]" />
    </button>
    <ul
      tabindex="0"
      class="dropdown-content mt-2 p-1.5 w-32 bg-base-100 rounded-xl shadow-lg border border-base-200"
    >
      <li v-for="lang in locales" :key="lang.code">
        <a
          @click="currentLocale = lang.code"
          class="flex items-center justify-between px-3 py-2 rounded-lg text-sm hover:bg-base-200 cursor-pointer"
          :class="{ 'bg-primary/5 text-primary font-medium': currentLocale === lang.code }"
        >
          <span>{{ lang.label }}</span>
          <span class="text-[10px] uppercase opacity-50">{{ lang.short }}</span>
        </a>
      </li>
    </ul>
  </div>
</template>
