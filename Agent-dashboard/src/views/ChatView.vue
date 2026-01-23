<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useI18n } from 'vue-i18n'
import AppIcon from '@/components/ui/AppIcon.vue'

const authStore = useAuthStore()
const { t } = useI18n()

const message = ref('')
const messages = ref<{ role: 'user' | 'assistant'; content: string; timestamp: Date }[]>([])
const isLoading = ref(false)
const chatContainerRef = ref<HTMLElement | null>(null)

// Auto-login if not authenticated
onMounted(async () => {
  await authStore.initialize()
  scrollToBottom()
})

const scrollToBottom = async () => {
  await nextTick()
  if (chatContainerRef.value) {
    chatContainerRef.value.scrollTop = chatContainerRef.value.scrollHeight
  }
}

watch(
  messages,
  () => {
    scrollToBottom()
  },
  { deep: true },
)

const sendMessage = async () => {
  if (!message.value.trim() || isLoading.value) return

  const userMessage = message.value.trim()
  messages.value.push({ role: 'user', content: userMessage, timestamp: new Date() })
  message.value = ''
  isLoading.value = true

  try {
    const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/chat/send`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authStore.accessToken}`,
      },
      body: JSON.stringify({
        message: userMessage,
        sessionId: localStorage.getItem('chatSessionId') || crypto.randomUUID(),
        platform: 'web',
      }),
    })

    const data = await response.json()
    messages.value.push({ role: 'assistant', content: data.response, timestamp: new Date() })
  } catch (error) {
    messages.value.push({ role: 'assistant', content: t('chat.error'), timestamp: new Date() })
  } finally {
    isLoading.value = false
  }
}

const handleLogin = () => {
  authStore.login()
}

function formatTime(date: Date) {
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

function setQuickAction(text: string) {
  message.value = text
  sendMessage()
}
</script>

<template>
  <div
    class="h-[calc(100vh-10.5rem)] flex flex-col bg-base-100 rounded-2xl border border-base-200 overflow-hidden"
  >
    <!-- Chat Header -->
    <div
      class="flex-none p-3 px-5 border-b border-base-100 flex items-center justify-between bg-base-100 z-10"
    >
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
          <AppIcon name="sparkles" size="sm" />
        </div>
        <div>
          <h2 class="font-semibold text-sm">AI Assistant</h2>
          <div class="flex items-center gap-1.5">
            <span class="w-1.5 h-1.5 rounded-full bg-success animate-pulse"></span>
            <p class="text-[10px] text-base-content/50 uppercase tracking-wide font-medium">
              Online
            </p>
          </div>
        </div>
      </div>

      <div
        v-if="authStore.isAuthenticated"
        class="badge badge-sm badge-ghost gap-1 font-medium bg-base-200/50 border-0 text-xs"
      >
        {{ authStore.userName }}
      </div>
    </div>

    <!-- Login Required -->
    <div
      v-if="!authStore.isAuthenticated"
      class="flex-1 flex flex-col items-center justify-center p-8 bg-base-50"
    >
      <div class="w-16 h-16 rounded-2xl bg-base-200 flex items-center justify-center mb-4">
        <AppIcon name="user" size="xl" class="text-base-content/30" />
      </div>
      <h3 class="text-lg font-semibold mb-2">Authentication Required</h3>
      <p class="text-base-content/60 mb-6 text-center max-w-xs">
        Please sign in to start chatting with the AI Assistant.
      </p>
      <button @click="handleLogin" class="btn btn-primary rounded-xl px-6">Sign In with SSO</button>
    </div>

    <!-- Chat Area -->
    <template v-else>
      <div
        ref="chatContainerRef"
        class="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6 scroll-smooth bg-gradient-to-b from-base-100 to-base-50/50"
      >
        <!-- Welcome Message -->
        <div
          v-if="messages.length === 0"
          class="flex flex-col items-center justify-center h-full text-center space-y-6 pb-10"
        >
          <div
            class="w-16 h-16 rounded-2xl bg-white shadow-lg shadow-base-200 flex items-center justify-center mb-2 border border-base-100"
          >
            <AppIcon name="sparkles" size="lg" class="text-primary" />
          </div>
          <div class="max-w-md space-y-1">
            <h3 class="text-lg font-semibold">{{ t('chat.welcome') }}</h3>
            <p class="text-sm text-base-content/60">
              I can help you analyze data, answer questions, or assist with your daily tasks.
            </p>
          </div>
          <div class="grid grid-cols-2 gap-3 w-full max-w-md mt-4">
            <button
              @click="setQuickAction('Analyze the latest trends')"
              class="group p-3 text-sm text-left bg-base-100 hover:bg-white rounded-xl transition-all border border-base-200 hover:border-primary/30 hover:shadow-md flex items-center gap-3"
            >
              <div
                class="w-8 h-8 rounded-lg bg-primary/5 group-hover:bg-primary/10 flex items-center justify-center text-primary transition-colors"
              >
                <AppIcon name="chart-bar" class="w-4 h-4" />
              </div>
              <span class="text-xs font-medium text-base-content/80 group-hover:text-base-content"
                >Analyze trends</span
              >
            </button>
            <button
              @click="setQuickAction('Help me write a report')"
              class="group p-3 text-sm text-left bg-base-100 hover:bg-white rounded-xl transition-all border border-base-200 hover:border-secondary/30 hover:shadow-md flex items-center gap-3"
            >
              <div
                class="w-8 h-8 rounded-lg bg-secondary/5 group-hover:bg-secondary/10 flex items-center justify-center text-secondary transition-colors"
              >
                <AppIcon name="document-text" class="w-4 h-4" />
              </div>
              <span class="text-xs font-medium text-base-content/80 group-hover:text-base-content"
                >Write a report</span
              >
            </button>
            <button
              @click="setQuickAction('Check system status')"
              class="group p-3 text-sm text-left bg-base-100 hover:bg-white rounded-xl transition-all border border-base-200 hover:border-accent/30 hover:shadow-md flex items-center gap-3"
            >
              <div
                class="w-8 h-8 rounded-lg bg-accent/5 group-hover:bg-accent/10 flex items-center justify-center text-accent transition-colors"
              >
                <AppIcon name="server" class="w-4 h-4" />
              </div>
              <span class="text-xs font-medium text-base-content/80 group-hover:text-base-content"
                >System status</span
              >
            </button>
            <button
              @click="setQuickAction('What can you do?')"
              class="group p-3 text-sm text-left bg-base-100 hover:bg-white rounded-xl transition-all border border-base-200 hover:border-warning/30 hover:shadow-md flex items-center gap-3"
            >
              <div
                class="w-8 h-8 rounded-lg bg-warning/5 group-hover:bg-warning/10 flex items-center justify-center text-warning transition-colors"
              >
                <AppIcon name="light-bulb" class="w-4 h-4" />
              </div>
              <span class="text-xs font-medium text-base-content/80 group-hover:text-base-content"
                >Capabilities</span
              >
            </button>
          </div>
        </div>

        <!-- Messages -->
        <div
          v-for="(msg, index) in messages"
          :key="index"
          class="flex w-full"
          :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
        >
          <div
            class="flex max-w-[85%] sm:max-w-[75%] gap-3"
            :class="msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'"
          >
            <!-- Avatar -->
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 mt-1 shadow-sm border border-black/5"
              :class="
                msg.role === 'user'
                  ? 'bg-white'
                  : 'bg-gradient-to-br from-primary to-secondary text-white'
              "
            >
              <span v-if="msg.role === 'user'" class="text-xs font-bold text-base-content/70"
                >You</span
              >
              <AppIcon v-else name="sparkles" class="w-4 h-4" />
            </div>

            <!-- Bubble -->
            <div
              class="flex flex-col gap-1"
              :class="msg.role === 'user' ? 'items-end' : 'items-start'"
            >
              <div
                class="px-4 py-2.5 rounded-2xl text-sm shadow-sm leading-relaxed whitespace-pre-wrap border"
                :class="[
                  msg.role === 'user'
                    ? 'bg-base-100 text-base-content border-base-200 rounded-tr-sm'
                    : 'bg-white text-base-content border-base-200 rounded-tl-sm',
                ]"
              >
                {{ msg.content }}
              </div>
              <span class="text-[10px] text-base-content/30 px-1">{{
                formatTime(msg.timestamp)
              }}</span>
            </div>
          </div>
        </div>

        <!-- Loading Indicator -->
        <div v-if="isLoading" class="flex w-full justify-start">
          <div class="flex max-w-[85%] gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-gradient-to-br from-primary to-secondary text-white flex items-center justify-center flex-shrink-0 mt-1 shadow-sm"
            >
              <AppIcon name="sparkles" class="w-4 h-4" />
            </div>
            <div
              class="bg-white border border-base-200 px-4 py-3 rounded-2xl rounded-tl-sm shadow-sm"
            >
              <div class="flex gap-1.5">
                <span
                  class="w-1.5 h-1.5 bg-base-content/40 rounded-full animate-bounce"
                  style="animation-delay: 0ms"
                ></span>
                <span
                  class="w-1.5 h-1.5 bg-base-content/40 rounded-full animate-bounce"
                  style="animation-delay: 150ms"
                ></span>
                <span
                  class="w-1.5 h-1.5 bg-base-content/40 rounded-full animate-bounce"
                  style="animation-delay: 300ms"
                ></span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Input Area -->
      <div class="flex-none p-5 bg-base-100/80 backdrop-blur-sm border-t border-base-200">
        <div class="relative flex items-center gap-2 max-w-3xl mx-auto">
          <div class="relative flex-1 group">
            <textarea
              v-model="message"
              @keydown.enter.exact.prevent="sendMessage"
              @input="
                (e) => {
                  const target = e.target as HTMLTextAreaElement
                  target.style.height = 'auto'
                  target.style.height = Math.min(target.scrollHeight, 120) + 'px'
                }
              "
              class="textarea textarea-bordered w-full pr-12 pl-6 rounded-3xl text-sm shadow-sm border-base-200 bg-base-200/30 focus:bg-base-100 focus:border-primary/30 focus:shadow-md transition-all resize-none py-3 leading-5 custom-scrollbar"
              :placeholder="t('chat.placeholder', 'Ask me anything...')"
              :disabled="isLoading"
              rows="1"
              style="min-height: 3rem; height: 3rem"
            ></textarea>
            <button
              class="absolute right-3 bottom-2.5 btn btn-ghost btn-sm btn-circle hover:bg-base-200 text-base-content/40 hover:text-primary transition-colors"
              title="Attach file (Demo)"
            >
              <AppIcon name="paper-clip" class="w-4 h-4" />
            </button>
          </div>

          <button
            @click="sendMessage"
            class="btn btn-primary btn-circle shadow-lg shadow-primary/25 hover:shadow-primary/40 hover:scale-105 transition-all text-white"
            :disabled="isLoading || !message.trim()"
          >
            <AppIcon name="arrow-up" class="w-5 h-5" />
          </button>
        </div>
        <p class="text-[10px] text-center text-base-content/30 mt-3 font-medium">
          AI can make mistakes. Please check important info.
        </p>
      </div>
    </template>
  </div>
</template>

<style scoped>
/* Custom scrollbar for chat area */
.overflow-y-auto::-webkit-scrollbar {
  width: 4px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  background: transparent;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  background: transparent;
  border-radius: 10px;
}

.overflow-y-auto:hover::-webkit-scrollbar-thumb {
  background: oklch(var(--bc) / 0.1);
}
</style>
