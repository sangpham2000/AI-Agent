<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

// Helper to get locale key from item name
const getItemKey = (item: string) => {
  const map: Record<string, string> = {
    Documentation: 'documentation',
    'API Reference': 'apiReference',
    Blog: 'blog',
    Community: 'community',
  }
  return map[item] || item.toLowerCase()
}
</script>

<template>
  <div class="min-h-screen pt-24 pb-12 relative overflow-hidden">
    <div class="absolute inset-0 bg-grid-pattern opacity-[0.3] z-0 pointer-events-none"></div>

    <div class="container mx-auto px-6 relative z-10">
      <div class="text-center max-w-3xl mx-auto mb-16">
        <h1 class="text-4xl md:text-5xl font-bold mb-6">
          {{ t('resourcesPage.title') }}
          <span class="text-info">{{ t('resourcesPage.highlight') }}</span>
        </h1>
        <p class="text-lg text-base-content/70">{{ t('resourcesPage.subtitle') }}</p>
      </div>

      <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div
          v-for="item in ['Documentation', 'API Reference', 'Blog', 'Community']"
          :key="item"
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-6 hover:border-info/30 cursor-pointer transition-all"
        >
          <h3 class="font-bold text-lg mb-2">{{ t(`resourcesPage.cards.${getItemKey(item)}`) }}</h3>
          <p class="text-sm text-base-content/60">
            {{
              t('resourcesPage.cards.description', {
                item: t(`resourcesPage.cards.${getItemKey(item)}`).toLowerCase(),
              })
            }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
