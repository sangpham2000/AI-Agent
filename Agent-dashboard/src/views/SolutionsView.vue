<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
</script>

<template>
  <div class="py-24 relative bg-base-200/30">
    <div class="container mx-auto px-6 relative z-10">
      <div class="text-center max-w-3xl mx-auto mb-16">
        <h1 class="text-4xl md:text-5xl font-bold mb-6">
          {{ t('solutionsPage.title') }}&nbsp;
          <span class="text-secondary">{{ t('solutionsPage.highlight') }}</span>
        </h1>
        <p class="text-lg text-base-content/70">
          {{ t('solutionsPage.subtitle') }}
        </p>
      </div>

      <div class="grid md:grid-cols-2 gap-8">
        <div
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-8 hover:border-secondary/30 transition-all"
        >
          <h3 class="text-2xl font-bold mb-4">{{ t('solutionsPage.education.title') }}</h3>
          <p class="text-base-content/60 mb-6">
            {{ t('solutionsPage.education.description') }}
          </p>
          <button class="btn btn-outline btn-secondary btn-sm">
            {{ t('solutionsPage.learnMore') }}
          </button>
        </div>
        <div
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-8 hover:border-secondary/30 transition-all"
        >
          <h3 class="text-2xl font-bold mb-4">{{ t('solutionsPage.enterprise.title') }}</h3>
          <p class="text-base-content/60 mb-6">
            {{ t('solutionsPage.enterprise.description') }}
          </p>
          <button class="btn btn-outline btn-secondary btn-sm">
            {{ t('solutionsPage.learnMore') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
