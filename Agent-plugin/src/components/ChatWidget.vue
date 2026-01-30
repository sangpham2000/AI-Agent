<script setup lang="ts">
import { useChatStore } from '@/stores/chat'
import ChatWindow from './ChatWindow.vue'
import { storeToRefs } from 'pinia'
import { inject } from 'vue'

const store = useChatStore()
const { isOpen } = storeToRefs(store)
const widgetOptions = inject('widgetOptions', { theme: 'light' })
</script>

<template>
  <div class="fixed bottom-6 right-6 z-[9999] font-sans antialiased text-base">
    <!-- Chat Window -->
    <transition
      enter-active-class="transition ease-out duration-300"
      enter-from-class="opacity-0 translate-y-4 scale-95"
      enter-to-class="opacity-100 translate-y-0 scale-100"
      leave-active-class="transition ease-in duration-200"
      leave-from-class="opacity-100 translate-y-0 scale-100"
      leave-to-class="opacity-0 translate-y-4 scale-95"
    >
      <ChatWindow v-if="isOpen" class="mb-4" />
    </transition>

    <!-- Launcher Button -->
    <button
      v-if="!isOpen"
      @click="store.toggleOpen()"
      class="btn btn-primary btn-circle w-14 h-14 shadow-lg hover:scale-110 transition-transform duration-200 flex items-center justify-center p-0"
    >
      <i class="pi pi-comments text-2xl"></i>
    </button>
  </div>
</template>

<style scoped>
/* Ensure styles don't bleed out too much, though Shadow DOM is better for true isolation. 
   Since we aren't using Shadow DOM here (standard Vue mount), we rely on scoping and high z-index.
*/
</style>
