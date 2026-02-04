<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
</script>

<template>
  <div class="py-24 relative">
    <div class="container mx-auto px-6 relative z-10">
      <div class="text-center max-w-3xl mx-auto mb-16">
        <h1 class="text-4xl md:text-5xl font-bold mb-6">
          {{ t('pricingPage.title') }}
          <span class="text-accent">{{ t('pricingPage.highlight') }}</span>
        </h1>
        <p class="text-lg text-base-content/70">{{ t('pricingPage.subtitle') }}</p>
      </div>

      <div class="grid md:grid-cols-3 gap-8 max-w-5xl mx-auto">
        <!-- Free -->
        <div
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-6 hover:shadow-lg transition-all"
        >
          <h3 class="text-xl font-bold mb-2">{{ t('pricingPage.starter.title') }}</h3>
          <div class="text-3xl font-bold mb-4">
            {{ t('pricingPage.starter.price') }}
            <span class="text-sm font-normal text-base-content/50">{{
              t('pricingPage.month')
            }}</span>
          </div>
          <ul class="space-y-3 mb-8 text-sm text-base-content/70">
            <li class="flex gap-2">✓ {{ t('pricingPage.starter.features[0]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.starter.features[1]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.starter.features[2]') }}</li>
          </ul>
          <button class="btn btn-outline w-full">{{ t('pricingPage.getStarted') }}</button>
        </div>

        <!-- Pro -->
        <div
          class="card bg-base-100 backdrop-blur-xl border border-primary p-6 shadow-xl relative scale-105 z-10"
        >
          <div
            class="absolute top-0 right-0 bg-primary text-white text-xs px-3 py-1 rounded-bl-xl rounded-tr-xl font-bold"
          >
            {{ t('pricingPage.pro.tag') }}
          </div>
          <h3 class="text-xl font-bold mb-2 text-primary">{{ t('pricingPage.pro.title') }}</h3>
          <div class="text-3xl font-bold mb-4">
            {{ t('pricingPage.pro.price') }}
            <span class="text-sm font-normal text-base-content/50">{{
              t('pricingPage.month')
            }}</span>
          </div>
          <ul class="space-y-3 mb-8 text-sm text-base-content/70">
            <li class="flex gap-2">✓ {{ t('pricingPage.pro.features[0]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.pro.features[1]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.pro.features[2]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.pro.features[3]') }}</li>
          </ul>
          <button class="btn btn-primary w-full">{{ t('pricingPage.pro.cta') }}</button>
        </div>

        <!-- Enterprise -->
        <div
          class="card bg-base-100/50 backdrop-blur-md border border-base-200 p-6 hover:shadow-lg transition-all"
        >
          <h3 class="text-xl font-bold mb-2">{{ t('pricingPage.enterprise.title') }}</h3>
          <div class="text-3xl font-bold mb-4">{{ t('pricingPage.enterprise.price') }}</div>
          <ul class="space-y-3 mb-8 text-sm text-base-content/70">
            <li class="flex gap-2">✓ {{ t('pricingPage.enterprise.features[0]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.enterprise.features[1]') }}</li>
            <li class="flex gap-2">✓ {{ t('pricingPage.enterprise.features[2]') }}</li>
          </ul>
          <button class="btn btn-outline w-full">{{ t('pricingPage.enterprise.cta') }}</button>
        </div>
      </div>
    </div>
  </div>
</template>
