<script setup lang="ts">
import AppIcon from './AppIcon.vue'

defineProps<{
  isOpen: boolean
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  type?: 'danger' | 'warning' | 'info'
  isLoading?: boolean
}>()

const emit = defineEmits(['close', 'confirm'])
</script>

<template>
  <div class="modal z-50" :class="{ 'modal-open': isOpen }">
    <div class="modal-box relative">
      <button
        @click="emit('close')"
        class="btn btn-sm btn-circle btn-ghost absolute right-2 top-2"
        :disabled="isLoading"
      >
        ✕
      </button>

      <div class="flex items-start gap-4">
        <!-- Icon -->
        <div
          class="flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center"
          :class="{
            'bg-error/10 text-error': type === 'danger' || !type,
            'bg-warning/10 text-warning': type === 'warning',
            'bg-info/10 text-info': type === 'info',
          }"
        >
          <AppIcon v-if="type === 'danger' || !type" name="trash" class="w-5 h-5" />
          <AppIcon v-else-if="type === 'warning'" name="alert-triangle" class="w-5 h-5" />
          <AppIcon v-else name="information-circle" class="w-5 h-5" />
        </div>

        <div class="flex-1">
          <h3 class="font-bold text-lg">{{ title }}</h3>
          <p class="py-2 text-sm text-base-content/70">
            {{ message }}
          </p>
        </div>
      </div>

      <div class="modal-action">
        <button class="btn" @click="emit('close')" :disabled="isLoading">
          {{ cancelText || 'Cancel' }}
        </button>
        <button
          class="btn"
          :class="{
            'btn-error': type === 'danger' || !type,
            'btn-warning': type === 'warning',
            'btn-info': type === 'info',
          }"
          @click="emit('confirm')"
          :disabled="isLoading"
        >
          <span v-if="isLoading" class="loading loading-spinner loading-xs"></span>
          {{ confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
    <div class="modal-backdrop bg-black/40" @click="!isLoading && emit('close')"></div>
  </div>
</template>
