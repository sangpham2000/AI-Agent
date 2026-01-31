import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { conversationsApi, chatApi } from '@/api'
import type { ConversationSummary, ConversationDetail } from '@/api'

export const useConversationsStore = defineStore('conversations', () => {
  // State
  const conversations = ref<ConversationSummary[]>([])
  const selectedConversation = ref<ConversationDetail | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)
  const totalPages = ref(0)
  const searchQuery = ref('')
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

  // Group conversations by date relative to today
  const groupedConversations = computed(() => {
    const groups: Record<string, ConversationSummary[]> = {
      Today: [],
      Yesterday: [],
      'Previous 7 Days': [],
      Older: [],
    }

    const now = new Date()
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime()
    const yesterday = today - 86400000
    const lastWeek = today - 86400000 * 7

    conversations.value.forEach((conv) => {
      const c = conv as any
      const convDate = new Date(c.updatedAt || c.startedAt).getTime() // Prioritize updatedAt
      if (convDate >= today) {
        if (!groups['Today']) groups['Today'] = []
        groups['Today'].push(conv)
      } else if (convDate >= yesterday) {
        if (!groups['Yesterday']) groups['Yesterday'] = []
        groups['Yesterday'].push(conv)
      } else if (convDate >= lastWeek) {
        if (!groups['Previous 7 Days']) groups['Previous 7 Days'] = []
        groups['Previous 7 Days'].push(conv)
      } else {
        if (!groups['Older']) groups['Older'] = []
        groups['Older'].push(conv)
      }
    })

    // Remove empty groups and return as array of { label, items }
    return Object.entries(groups)
      .filter(([_, items]) => items.length > 0)
      .map(([label, items]) => ({ label, items }))
  })

  // Actions
  async function fetchConversations(params?: {
    platform?: string
    search?: string
    startDate?: string
    endDate?: string
    page?: number
    pageSize?: number
    append?: boolean
  }) {
    const isAppend = params?.append || false

    if (!isAppend) {
      isLoading.value = true
    }

    // Update local state if params provided
    if (params?.search !== undefined) searchQuery.value = params.search

    const queryParams = {
      ...params,
      search: searchQuery.value || params?.search,
      page: params?.page || 1,
      pageSize: params?.pageSize || 20,
    }

    error.value = null
    try {
      const response = await conversationsApi.list(queryParams)

      if (isAppend) {
        // Filter out duplicates just in case
        const newItems = response.data.items.filter(
          (newItem) => !conversations.value.some((existing) => existing.id === newItem.id),
        )
        conversations.value = [...conversations.value, ...newItems]
      } else {
        conversations.value = response.data.items
      }

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

  async function fetchUserConversations(params?: {
    page?: number
    pageSize?: number
    append?: boolean
  }) {
    const isAppend = params?.append || false
    if (!isAppend) {
      isLoading.value = true
    }

    const queryParams = {
      page: params?.page || 1,
      pageSize: params?.pageSize || 20,
    }

    error.value = null
    try {
      // Use chatApi for user context
      const response = await chatApi.list(queryParams)

      if (isAppend) {
        const newItems = response.data.items.filter(
          (newItem) => !conversations.value.some((existing) => existing.id === newItem.id),
        )
        conversations.value = [...conversations.value, ...newItems]
      } else {
        conversations.value = response.data.items
      }

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

  async function loadMore() {
    if (currentPage.value < totalPages.value && !isLoading.value) {
      // Logic for loadMore needs to know WHICH fetch to call (User vs Admin)
      // For now, let's assume if we used fetchUserConversations before, we continue using it?
      // Or we can check if we have admin filters.
      // But simpler: ChatLayout calls fetchUserConversations, so we should probably have loadMoreUserConversations?
      // Or make loadMore smart.
      // If searchQuery or filtered params are set, maybe use fetchConversations.
      // If simple pagination, maybe use fetchUserConversations?
      // To be safe, ChatLayout should handle loadMore explicitly or pass a flag.
      // Let's defaulted to fetchConversations for backward compatibility, but ChatLayout uses infinite scroll calling fetchUserConversations manually?
      // Actually ChatLayout calls loadMore() in existing code?
      // Let's check ChatLayout later. For now, keep loadMore as is (calling fetchConversations).
      await fetchConversations({
        page: currentPage.value + 1,
        search: searchQuery.value,
        append: true,
      })
    }
  }

  async function fetchConversationById(id: string) {
    isLoading.value = true
    error.value = null
    try {
      // Try chatApi first for User (ChatView)
      try {
        const response = await chatApi.getById(id)
        selectedConversation.value = response.data
        return response.data
      } catch (e) {
        // Fallback to conversationsApi (Admin)
        const response = await conversationsApi.getById(id)
        selectedConversation.value = response.data
        return response.data
      }
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
      await chatApi.delete(id)
      conversations.value = conversations.value.filter((c) => c.id !== id)
      successMessage.value = 'Conversation deleted successfully'
      return true
    } catch (e: any) {
      if (e.response?.status === 403 || e.response?.status === 404) {
        try {
          await conversationsApi.delete(id)
          conversations.value = conversations.value.filter((c) => c.id !== id)
          successMessage.value = 'Conversation deleted successfully'
          return true
        } catch (e2: any) {
          error.value = e2.response?.data?.message || e2.message || 'Failed to delete conversation'
          return false
        }
      }
      error.value = e.response?.data?.message || e.message || 'Failed to delete conversation'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function deleteAllConversations() {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      await chatApi.deleteAll()
      conversations.value = []
      selectedConversation.value = null
      successMessage.value = 'All conversations deleted successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to delete conversations'
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
    groupedConversations,
    currentPage,
    pageSize,
    totalPages,
    isLoading,
    isExporting,
    error,
    successMessage,
    searchQuery,
    fetchConversations,
    fetchUserConversations,
    loadMore,
    fetchConversationById,
    exportConversation,
    exportConversations,
    deleteConversation,
    deleteAllConversations,
    clearError,
    clearMessages,
    setSelectedConversation,
  }
})
