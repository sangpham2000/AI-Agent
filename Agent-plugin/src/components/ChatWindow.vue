<script setup lang="ts">
import { ref, nextTick, watch } from 'vue'
import { useChatStore } from '@/stores/chat'
import MessageItem from './MessageItem.vue'
import { storeToRefs } from 'pinia'

const store = useChatStore()
const { messages, isTyping } = storeToRefs(store)
const inputContent = ref('')
const messagesContainer = ref<HTMLElement | null>(null)

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

watch([messages, isTyping], scrollToBottom, { deep: true })

const handleSubmit = () => {
  if (!inputContent.value.trim()) return
  store.sendMessage(inputContent.value)
  inputContent.value = ''
}
</script>

<template>
  <div
    class="flex flex-col h-full bg-base-100 dark:bg-gray-900 border border-base-300 dark:border-gray-700 rounded-lg shadow-xl overflow-hidden w-[350px] sm:w-[400px]"
  >
    <!-- Header -->
    <div class="bg-primary text-primary-content p-4 flex justify-between items-center">
      <h3 class="font-bold text-lg">AI Assistant</h3>
      <button @click="store.toggleOpen()" class="btn btn-ghost btn-sm btn-circle">
        <i class="pi pi-times"></i>
      </button>
    </div>

    <!-- Messages -->
    <div ref="messagesContainer" class="flex-1 overflow-y-auto p-4 space-y-4 bg-opacity-50">
      <MessageItem
        v-for="msg in messages"
        :key="msg.id"
        :content="msg.content"
        :role="msg.role"
        :timestamp="msg.timestamp"
      />
      <div v-if="isTyping" class="flex justify-start">
        <div class="bg-base-200 rounded-lg rounded-bl-none p-3">
          <span class="loading loading-dots loading-sm"></span>
        </div>
      </div>
    </div>

    <!-- Input -->
    <div class="p-3 bg-base-100 border-t border-base-200">
      <form @submit.prevent="handleSubmit" class="flex gap-2">
        <input
          v-model="inputContent"
          type="text"
          placeholder="Type a message..."
          class="input input-bordered w-full h-10"
        />
        <button
          type="submit"
          class="btn btn-primary btn-sm h-10 w-10 p-0 rounded-lg flex items-center justify-center"
        >
          <i class="pi pi-send"></i>
        </button>
      </form>
    </div>
  </div>
</template>
