<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConversationsStore } from '@/stores/conversations'
import { useI18n } from 'vue-i18n'
import AppIcon from '@/components/ui/AppIcon.vue'
import usersApi from '@/api/users'
import type { UserQuota } from '@/api/types'
import MarkdownIt from 'markdown-it'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const conversationsStore = useConversationsStore()
const { t } = useI18n()

const md = new MarkdownIt({
  html: false,
  breaks: true,
  linkify: true,
})

const message = ref('')
const messages = ref<{ role: 'user' | 'assistant'; content: string; timestamp: Date }[]>([])
const isLoading = ref(false)
const chatContainerRef = ref<HTMLElement | null>(null)
const selectedModel = ref('Gemini')
const quota = ref<UserQuota | null>(null)

// Load conversation logic
const loadConversation = async () => {
  const id = route.query.id as string
  if (id) {
    isLoading.value = true
    try {
      const conv = await conversationsStore.fetchConversationById(id)
      if (conv) {
        messages.value = (conv.messages || []).map((m: any) => ({
          role: m.role,
          content: m.content,
          timestamp: new Date(m.timestamp || m.createdAt),
        }))
      }
    } catch (e) {
      console.error('Failed to load conversation', e)
    } finally {
      isLoading.value = false
      scrollToBottom()
    }
  } else {
    // New chat
    messages.value = []
    conversationsStore.setSelectedConversation(null)
  }
}

// Watch for route changes to reload/reset
watch(
  () => route.query.id,
  () => {
    loadConversation()
  },
)

const fetchQuota = async () => {
  if (authStore.dbUser?.id) {
    try {
      const response = await usersApi.getQuota(authStore.dbUser.id)
      quota.value = response.data
    } catch (e) {
      console.error('Failed to fetch quota', e)
    }
  }
}

// Auto-login if not authenticated
onMounted(async () => {
  await authStore.initialize()
  if (authStore.isAuthenticated) {
    await fetchQuota()
    loadConversation()
  }
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
    const currentConvId = route.query.id || conversationsStore.selectedConversation?.id

    const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/chat/send`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${authStore.accessToken}`,
      },
      body: JSON.stringify({
        conversationId: currentConvId,
        message: userMessage,
        sessionId: currentConvId
          ? undefined
          : localStorage.getItem('chatSessionId') || crypto.randomUUID(),
        platform: 'web',
        model: selectedModel.value,
      }),
    })

    const data = await response.json()
    messages.value.push({ role: 'assistant', content: data.response, timestamp: new Date() })

    if (!currentConvId && data.conversationId) {
      conversationsStore.fetchConversations()
      router.replace({ query: { ...route.query, id: data.conversationId } })
    }

    await fetchQuota()
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

function renderMessageContent(content: string) {
  if (!content) return ''
  return md.render(content)
}
</script>

<template>
  <div class="flex-1 flex flex-col h-full overflow-hidden bg-base-100 relative">
    <!-- Chat Header -->
    <div
      class="flex-none h-14 px-6 border-b border-base-200 flex items-center justify-between bg-base-100/90 backdrop-blur-sm z-10 sticky top-0"
    >
      <div class="flex items-center gap-3">
        <h2 class="font-semibold text-sm text-base-content">AI Assistant</h2>
        <div class="flex items-center gap-1.5 px-2 py-0.5 rounded-full bg-success/10">
          <span class="relative flex h-1.5 w-1.5">
            <span
              class="animate-ping absolute inline-flex h-full w-full rounded-full bg-success opacity-75"
            ></span>
            <span class="relative inline-flex rounded-full h-1.5 w-1.5 bg-success"></span>
          </span>
          <span class="text-[10px] text-success font-medium">Online</span>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <select
          v-if="authStore.isAuthenticated"
          v-model="selectedModel"
          class="select select-sm select-ghost w-24 font-normal text-xs h-8 min-h-0 bg-transparent focus:bg-base-200 border-transparent focus:border-transparent hover:bg-base-200"
        >
          <option value="Gemini">Gemini</option>
          <option value="OpenAI">OpenAI</option>
        </select>
      </div>
    </div>

    <!-- Login Required -->
    <div
      v-if="!authStore.isAuthenticated"
      class="flex-1 flex flex-col items-center justify-center p-8"
    >
      <div class="w-16 h-16 rounded-2xl bg-base-200 flex items-center justify-center mb-4">
        <AppIcon name="user" class="w-6 h-6 text-base-content/40" />
      </div>
      <h3 class="text-lg font-semibold mb-2 text-base-content">Sign in to chat</h3>
      <p class="text-base-content/60 mb-6 text-center max-w-xs text-sm">
        Connect to your account to save your history.
      </p>
      <button @click="handleLogin" class="btn btn-neutral btn-sm rounded-lg px-6 font-medium">
        Sign In
      </button>
    </div>

    <!-- Chat Area -->
    <template v-else>
      <div
        ref="chatContainerRef"
        class="flex-1 overflow-y-auto px-4 sm:px-0 py-6 scroll-smooth custom-scrollbar"
      >
        <div class="max-w-3xl mx-auto flex flex-col gap-6">
          <!-- Welcome Message -->
          <div
            v-if="messages.length === 0"
            class="flex flex-col items-center justify-center py-16 text-center"
          >
            <div class="w-12 h-12 rounded-xl bg-base-200 flex items-center justify-center mb-6">
              <AppIcon name="sparkles" class="w-6 h-6 text-base-content" />
            </div>

            <h3 class="text-xl font-medium mb-2 text-base-content">
              Good
              {{
                new Date().getHours() < 12
                  ? 'morning'
                  : new Date().getHours() < 18
                    ? 'afternoon'
                    : 'evening'
              }}, {{ authStore.user?.profile?.name?.split(' ')[0] || 'User' }}
            </h3>
            <p class="text-base-content/60 max-w-sm mx-auto mb-10 text-sm">
              How can I help you today?
            </p>

            <div class="grid grid-cols-2 gap-2 w-full max-w-[400px]">
              <button
                v-for="(action, idx) in [
                  { icon: 'chart-bar', label: 'Analyze trends' },
                  { icon: 'document-text', label: 'Draft a report' },
                  { icon: 'code-bracket', label: 'Explain code' },
                  { icon: 'light-bulb', label: 'Brainstorm ideas' },
                ]"
                :key="idx"
                @click="setQuickAction(action.label)"
                class="group p-3 text-left bg-transparent hover:bg-base-200 rounded-xl border border-base-300 hover:border-base-content/20 transition-all flex items-center gap-3"
              >
                <AppIcon
                  :name="action.icon"
                  class="w-4 h-4 text-base-content/40 group-hover:text-base-content/60"
                />
                <span
                  class="text-xs font-medium text-base-content/60 group-hover:text-base-content"
                >
                  {{ action.label }}
                </span>
              </button>
            </div>
          </div>

          <!-- Messages -->
          <div
            v-for="(msg, index) in messages"
            :key="index"
            class="group w-full max-w-3xl mx-auto animate-in fade-in duration-300"
          >
            <div
              class="flex gap-4 p-2 rounded-xl transition-colors hover:bg-base-200/50"
              :class="msg.role === 'user' ? 'flex-row-reverse' : 'flex-row'"
            >
              <!-- Avatar -->
              <div
                class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center mt-0.5"
                :class="
                  msg.role === 'user' ? 'bg-base-200' : 'bg-transparent border border-base-200'
                "
              >
                <span
                  v-if="msg.role === 'user'"
                  class="text-[10px] font-medium text-base-content/60"
                  >You</span
                >
                <AppIcon v-else name="sparkles" class="w-4 h-4 text-base-content" />
              </div>

              <!-- Content -->
              <div
                class="flex flex-col gap-1 min-w-0 max-w-[85%]"
                :class="msg.role === 'user' ? 'items-end' : 'items-start'"
              >
                <div class="flex items-center gap-2 px-1">
                  <span class="text-xs font-medium text-base-content">
                    {{ msg.role === 'user' ? 'You' : 'AI Assistant' }}
                  </span>
                </div>

                <div
                  class="text-sm leading-7"
                  :class="[
                    msg.role === 'user'
                      ? 'bg-base-200 px-4 py-2 rounded-2xl rounded-tr-sm text-base-content'
                      : 'text-base-content px-1',
                  ]"
                >
                  <!-- Markdown Content -->
                  <div
                    v-if="msg.role === 'assistant'"
                    class="markdown-body prose prose-sm max-w-none prose-p:my-1 prose-headings:my-2 prose-code:text-base-content prose-code:bg-base-200 prose-code:px-1 prose-code:rounded prose-code:font-normal prose-pre:bg-base-300 prose-pre:text-base-content"
                    v-html="renderMessageContent(msg.content)"
                  ></div>
                  <p v-else class="whitespace-pre-wrap">{{ msg.content }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Loading Indicator -->
          <div v-if="isLoading" class="w-full max-w-3xl mx-auto flex gap-4 p-2">
            <div
              class="w-8 h-8 rounded-full border border-base-200 flex items-center justify-center flex-shrink-0 mt-0.5"
            >
              <AppIcon name="sparkles" class="w-4 h-4 text-base-content" />
            </div>
            <div class="flex items-center gap-1 mt-2">
              <span
                class="w-1.5 h-1.5 bg-base-content/30 rounded-full animate-bounce"
                style="animation-delay: 0ms"
              ></span>
              <span
                class="w-1.5 h-1.5 bg-base-content/30 rounded-full animate-bounce"
                style="animation-delay: 150ms"
              ></span>
              <span
                class="w-1.5 h-1.5 bg-base-content/30 rounded-full animate-bounce"
                style="animation-delay: 300ms"
              ></span>
            </div>
          </div>
        </div>

        <!-- Spacer for bottom input -->
        <div class="h-32"></div>
      </div>

      <!-- Input Floating Area -->
      <div class="absolute bottom-6 left-0 right-0 px-4 z-20">
        <div class="max-w-3xl mx-auto">
          <div
            class="bg-base-100 rounded-2xl border border-base-200 shadow-sm p-2 relative transition-all duration-300 ring-4 ring-transparent focus-within:ring-base-200 focus-within:border-base-300"
          >
            <div class="flex items-end gap-2">
              <textarea
                v-model="message"
                @keydown.enter.exact.prevent="sendMessage"
                @input="
                  (e) => {
                    const target = e.target as HTMLTextAreaElement
                    target.style.height = 'auto'
                    target.style.height = Math.min(target.scrollHeight, 150) + 'px'
                  }
                "
                class="block w-full bg-transparent border-0 focus:ring-0 p-3 min-h-[44px] max-h-[150px] resize-none text-base-content placeholder:text-base-content/40 custom-scrollbar text-sm leading-relaxed"
                :placeholder="t('chat.placeholder', 'Message AI Assistant...')"
                :disabled="isLoading"
                rows="1"
              ></textarea>

              <div class="flex items-center gap-1 pb-1.5 pr-1.5">
                <button
                  class="btn btn-circle btn-xs btn-ghost text-base-content/40 hover:text-base-content/60 hover:bg-base-200 transition-colors"
                  title="Attach file"
                >
                  <AppIcon name="paper-clip" class="w-4 h-4" />
                </button>
                <button
                  @click="sendMessage"
                  class="btn btn-sm btn-square rounded-lg bg-base-content hover:bg-base-content/90 text-base-100 transition-all duration-200 disabled:bg-base-300 disabled:text-base-content/40"
                  :disabled="isLoading || !message.trim()"
                >
                  <AppIcon name="arrow-up" class="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>

          <p class="text-[10px] text-center text-base-content/40 mt-3 font-medium">
            AI can make mistakes. Please check important info.
          </p>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
/* Custom scrollbar for chat area */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: oklch(var(--bc) / 0.2);
  border-radius: 10px;
}
.custom-scrollbar:hover::-webkit-scrollbar-thumb {
  background: oklch(var(--bc) / 0.4);
}

/* Markdown Styles Override using DaisyUI Semantic Colors */
:deep(.markdown-body) {
  font-size: 0.925rem;
  line-height: 1.6;
  color: oklch(var(--bc));
}
:deep(.markdown-body ul) {
  list-style-type: disc;
  padding-left: 1.5em;
}
:deep(.markdown-body ol) {
  list-style-type: decimal;
  padding-left: 1.5em;
}
:deep(.markdown-body strong) {
  font-weight: 600;
  color: inherit;
}
:deep(.markdown-body a) {
  color: oklch(var(--p));
  text-decoration: underline;
  text-underline-offset: 2px;
}
:deep(.markdown-body blockquote) {
  border-left: 4px solid oklch(var(--nc));
  color: oklch(var(--bc) / 0.6);
  padding-left: 1em;
}
</style>
