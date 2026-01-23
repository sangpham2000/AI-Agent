<script setup lang="ts">
defineProps<{
  title?: string
  description?: string
  loading?: boolean
}>()
</script>

<template>
  <div class="card bg-base-100 shadow-xl">
    <div class="card-body">
      <div v-if="title || $slots.header" class="flex items-center justify-between mb-4">
        <div>
          <h2 v-if="title" class="card-title text-lg">{{ title }}</h2>
          <p v-if="description" class="text-sm text-base-content/60">{{ description }}</p>
        </div>
        <div v-if="$slots.actions" class="flex items-center gap-2">
          <slot name="actions" />
        </div>
        <slot v-else name="header" />
      </div>

      <div v-if="loading" class="flex items-center justify-center py-8">
        <span class="loading loading-spinner loading-lg text-primary"></span>
      </div>

      <slot v-else />
    </div>
  </div>
</template>
