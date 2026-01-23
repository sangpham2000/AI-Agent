<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useI18n } from 'vue-i18n'
import LanguageSwitcher from '@/components/LanguageSwitcher.vue'

const authStore = useAuthStore()
const { t } = useI18n()

const message = ref('')
const messages = ref<{ role: 'user' | 'assistant'; content: string }[]>([])
const isLoading = ref(false)

// Auto-login if not authenticated
onMounted(async () => {
  await authStore.initialize()
})

const sendMessage = async () => {
  if (!message.value.trim() || isLoading.value) return

  const userMessage = message.value.trim()
  messages.value.push({ role: 'user', content: userMessage })
  message.value = ''
  isLoading.value = true

  try {
    // TODO: Call API with auth token
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
    messages.value.push({ role: 'assistant', content: data.response })
  } catch (error) {
    messages.value.push({ role: 'assistant', content: t('chat.error') })
  } finally {
    isLoading.value = false
  }
}

const handleLogin = () => {
  authStore.login()
}
</script>

<template>
  <div class="chat-widget">
    <!-- Header -->
    <div class="chat-header">
      <span><i class="pi pi-android"></i> {{ t('app.name') }}</span>
      <div class="header-actions">
        <LanguageSwitcher />
        <span v-if="authStore.isAuthenticated" class="user-badge">{{ authStore.userName }}</span>
      </div>
    </div>

    <!-- Login required -->
    <div v-if="!authStore.isAuthenticated" class="login-prompt">
      <p>Vui lòng đăng nhập để sử dụng</p>
      <button @click="handleLogin" class="login-btn">Đăng nhập SSO</button>
    </div>

    <!-- Chat content -->
    <template v-else>
      <div class="chat-messages">
        <div v-if="messages.length === 0" class="welcome-message">
          <p><i class="pi pi-hand-stop welcome-icon"></i></p>
          <p>{{ t('chat.welcome') }}</p>
          <p>Hãy đặt câu hỏi để bắt đầu.</p>
        </div>

        <div v-for="(msg, index) in messages" :key="index" :class="['message', msg.role]">
          {{ msg.content }}
        </div>

        <div v-if="isLoading" class="message assistant loading">
          <span class="typing-indicator"> <span></span><span></span><span></span> </span>
        </div>
      </div>

      <!-- Input -->
      <div class="chat-input">
        <input
          v-model="message"
          @keyup.enter="sendMessage"
          placeholder="Nhập câu hỏi..."
          :disabled="isLoading"
        />
        <button @click="sendMessage" :disabled="isLoading || !message.trim()">
          <i class="pi pi-send"></i>
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped>
.chat-widget {
  display: flex;
  flex-direction: column;
  width: 100%;
  max-width: 400px;
  height: 500px;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-weight: 600;
}

.user-badge {
  font-size: 0.8rem;
  background: rgba(255, 255, 255, 0.2);
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
}

.login-prompt {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: #f9fafb;
}

.login-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  background: #f9fafb;
}

.welcome-message {
  text-align: center;
  color: #666;
  padding: 2rem 0;
}

.message {
  max-width: 80%;
  padding: 0.75rem 1rem;
  margin-bottom: 0.75rem;
  border-radius: 16px;
  line-height: 1.4;
}

.message.user {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  margin-left: auto;
  border-bottom-right-radius: 4px;
}

.message.assistant {
  background: white;
  color: #333;
  border-bottom-left-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.typing-indicator {
  display: flex;
  gap: 4px;
}

.typing-indicator span {
  width: 8px;
  height: 8px;
  background: #999;
  border-radius: 50%;
  animation: bounce 1.4s infinite ease-in-out;
}

.typing-indicator span:nth-child(1) {
  animation-delay: -0.32s;
}
.typing-indicator span:nth-child(2) {
  animation-delay: -0.16s;
}

@keyframes bounce {
  0%,
  80%,
  100% {
    transform: scale(0);
  }
  40% {
    transform: scale(1);
  }
}

.chat-input {
  display: flex;
  padding: 0.75rem;
  background: white;
  border-top: 1px solid #eee;
}

.chat-input input {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 1px solid #e5e7eb;
  border-radius: 24px;
  outline: none;
  font-size: 0.9rem;
}

.chat-input button {
  margin-left: 0.5rem;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  cursor: pointer;
  font-size: 1rem;
}

.chat-input button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
