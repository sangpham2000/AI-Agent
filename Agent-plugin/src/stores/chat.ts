import { defineStore } from 'pinia'
import { ref } from 'vue'
import { sdk } from '@/sdk/AgentSDK'

export interface Message {
  id: string
  role: 'user' | 'assistant' | 'system'
  content: string
  timestamp: number
}

export const useChatStore = defineStore('chat', () => {
  const isOpen = ref(false)
  const messages = ref<Message[]>([])
  const isTyping = ref(false)
  const isConnected = ref(false)

  // Sync with SDK events
  sdk.on('open', () => (isOpen.value = true))
  sdk.on('close', () => (isOpen.value = false))
  sdk.on('connected', () => (isConnected.value = true))

  sdk.on('message', (data: any) => {
    isTyping.value = false
    const { user, message } = data
    addMessage({
      id: Date.now().toString(),
      role: user === 'User' ? 'user' : 'assistant',
      content: message,
      timestamp: Date.now(),
    })
  })

  function toggleOpen() {
    sdk.toggle()
  }

  function addMessage(msg: Message) {
    messages.value.push(msg)
  }

  async function sendMessage(content: string) {
    const userMsg: Message = {
      id: Date.now().toString(),
      role: 'user',
      content,
      timestamp: Date.now(),
    }
    addMessage(userMsg)

    isTyping.value = true
    await sdk.sendMessage(content)
  }

  return {
    isOpen,
    messages,
    isTyping,
    isConnected,
    toggleOpen,
    addMessage,
    sendMessage,
  }
})
