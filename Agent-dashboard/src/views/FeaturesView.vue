<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
</script>

<template>
  <div class="min-h-screen pt-24 pb-12 relative overflow-hidden">
    <!-- Dynamic Background -->
    <div class="absolute inset-0 bg-grid-pattern opacity-[0.3] z-0 pointer-events-none"></div>
    <div
      class="absolute top-[-10%] right-[-10%] w-[500px] h-[500px] bg-primary/10 rounded-full blur-3xl opacity-40"
    ></div>
    <div
      class="absolute bottom-[-10%] left-[-10%] w-[500px] h-[500px] bg-secondary/10 rounded-full blur-3xl opacity-40"
    ></div>

    <div class="container mx-auto px-6 relative z-10">
      <div class="text-center max-w-3xl mx-auto mb-16">
        <h1 class="text-4xl md:text-5xl font-bold mb-6">
          {{ t('featuresPage.title') }}
          <span class="text-primary">{{ t('featuresPage.highlight') }}</span>
        </h1>
        <p class="text-lg text-base-content/70">
          {{ t('featuresPage.subtitle') }}
        </p>
      </div>

      <div class="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
        <!-- Placeholder Feature Cards -->
        <div
          v-for="i in 6"
          :key="i"
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-6 hover:shadow-lg transition-all"
        >
          <div
            class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary mb-4"
          >
            <span class="font-bold text-xl">{{ i }}</span>
          </div>
          <h3 class="font-bold text-xl mb-2">{{ t('featuresPage.cards.title') }} {{ i }}</h3>
          <p class="text-base-content/60">
            {{ t('featuresPage.cards.description') }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
