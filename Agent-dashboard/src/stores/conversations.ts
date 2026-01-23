import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { conversationsApi } from '@/api'
import type { ConversationSummary, ConversationDetail } from '@/api'

export const useConversationsStore = defineStore('conversations', () => {
  // State
  const conversations = ref<ConversationSummary[]>([])
  const selectedConversation = ref<ConversationDetail | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)
  const totalPages = ref(0)
  const isLoading = ref(false)
  const isExporting = ref(false)
  const error = ref<string | null>(null)
  const successMessage = ref<string | null>(null)

  // Computed
  const totalMessages = computed(() => {
    return conversations.value.reduce((sum, c) => sum + c.messageCount, 0)
  })

  const averageMessagesPerConversation = computed(() => {
    if (conversations.value.length === 0) return 0
    return totalMessages.value / conversations.value.length
  })

  // Actions
  async function fetchConversations(params?: {
    platform?: string
    search?: string
    startDate?: string
    endDate?: string
    page?: number
    pageSize?: number
  }) {
    isLoading.value = true
    error.value = null
    try {
      const response = await conversationsApi.list(params)
      conversations.value = response.data.items
      totalCount.value = response.data.totalCount
      currentPage.value = response.data.pageNumber
      pageSize.value = response.data.pageSize
      totalPages.value = response.data.totalPages
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch conversations'
    } finally {
      isLoading.value = false
    }
  }

  async function fetchConversationById(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await conversationsApi.getById(id)
      selectedConversation.value = response.data
      return response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch conversation'
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function exportConversation(id: string, format: 'json' | 'csv' = 'json') {
    isExporting.value = true
    error.value = null
    try {
      const response = await conversationsApi.export(id, format)
      const blob = new Blob([response.data], {
        type: format === 'csv' ? 'text/csv' : 'application/json',
      })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `conversation-${id}.${format}`
      link.click()
      window.URL.revokeObjectURL(url)
      successMessage.value = 'Conversation exported successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to export conversation'
      return false
    } finally {
      isExporting.value = false
    }
  }

  async function exportConversations(platform?: string, startDate?: string, endDate?: string) {
    isExporting.value = true
    error.value = null
    try {
      // Export all conversations as JSON
      const data = conversations.value.filter((c) => {
        if (platform && c.platform !== platform) return false
        if (startDate && new Date(c.startedAt) < new Date(startDate)) return false
        if (endDate && new Date(c.startedAt) > new Date(endDate)) return false
        return true
      })
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `conversations-export-${new Date().toISOString().slice(0, 10)}.json`
      link.click()
      window.URL.revokeObjectURL(url)
      successMessage.value = `Exported ${data.length} conversations`
      return true
    } catch (e: any) {
      error.value = e.message || 'Failed to export conversations'
      return false
    } finally {
      isExporting.value = false
    }
  }

  async function deleteConversation(id: string) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      await conversationsApi.delete(id)
      conversations.value = conversations.value.filter((c) => c.id !== id)
      successMessage.value = 'Conversation deleted successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to delete conversation'
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearMessages() {
    error.value = null
    successMessage.value = null
  }

  function setSelectedConversation(conversation: ConversationDetail | null) {
    selectedConversation.value = conversation
  }

  return {
    conversations,
    selectedConversation,
    totalCount,
    totalMessages,
    averageMessagesPerConversation,
    currentPage,
    pageSize,
    totalPages,
    isLoading,
    isExporting,
    error,
    successMessage,
    fetchConversations,
    fetchConversationById,
    exportConversation,
    exportConversations,
    deleteConversation,
    clearError,
    clearMessages,
    setSelectedConversation,
  }
})
