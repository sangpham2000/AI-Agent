import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { analyticsApi } from '@/api'
import type {
  DashboardStats,
  ConversationTrend,
  PopularQuestion,
  DailyMessageCount,
  PlatformDistribution,
} from '@/api'

export const useAnalyticsStore = defineStore('analytics', () => {
  // State
  const dashboardStats = ref<DashboardStats | null>(null)
  const conversationTrends = ref<ConversationTrend[]>([])
  const popularQuestions = ref<PopularQuestion[]>([])
  const dailyMessageCounts = ref<DailyMessageCount[]>([])
  const platformDistribution = ref<PlatformDistribution[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const totalConversations = computed(() => dashboardStats.value?.totalConversations ?? 0)
  const conversationsToday = computed(() => dashboardStats.value?.conversationsToday ?? 0)
  const totalDocuments = computed(() => dashboardStats.value?.totalDocuments ?? 0)
  const documentsProcessed = computed(() => dashboardStats.value?.documentsProcessed ?? 0)
  const totalUsers = computed(() => dashboardStats.value?.totalUsers ?? 0)
  const activeUsers = computed(() => dashboardStats.value?.activeUsers ?? 0)
  const totalTokensUsedThisMonth = computed(
    () => dashboardStats.value?.totalTokensUsedThisMonth ?? 0,
  )
  const recentActivities = computed(() => dashboardStats.value?.recentActivities ?? [])
  const messagesThisWeek = computed(() => dashboardStats.value?.messagesThisWeek ?? [])

  // Actions
  async function fetchDashboardStats() {
    isLoading.value = true
    error.value = null
    try {
      const response = await analyticsApi.getDashboardStats()
      dashboardStats.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch dashboard stats'
    } finally {
      isLoading.value = false
    }
  }

  async function fetchConversationTrends(days: number = 7) {
    try {
      const response = await analyticsApi.getConversationTrends(days)
      conversationTrends.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message
    }
  }

  async function fetchPopularQuestions(limit: number = 10) {
    try {
      const response = await analyticsApi.getPopularQuestions(limit)
      popularQuestions.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message
    }
  }

  async function fetchDailyMessageCounts(days: number = 7) {
    try {
      const response = await analyticsApi.getDailyMessageCounts(days)
      dailyMessageCounts.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message
    }
  }

  async function fetchPlatformDistribution() {
    try {
      const response = await analyticsApi.getPlatformDistribution()
      platformDistribution.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message
    }
  }

  async function fetchAll() {
    isLoading.value = true
    error.value = null
    try {
      await Promise.all([
        fetchDashboardStats(),
        fetchConversationTrends(),
        fetchPopularQuestions(),
        fetchDailyMessageCounts(),
        fetchPlatformDistribution(),
      ])
    } catch (e: any) {
      error.value = e.message
    } finally {
      isLoading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  return {
    // State
    dashboardStats,
    conversationTrends,
    popularQuestions,
    dailyMessageCounts,
    platformDistribution,
    isLoading,
    error,
    // Getters
    totalConversations,
    conversationsToday,
    totalDocuments,
    documentsProcessed,
    totalUsers,
    activeUsers,
    totalTokensUsedThisMonth,
    recentActivities,
    messagesThisWeek,
    // Actions
    fetchDashboardStats,
    fetchConversationTrends,
    fetchPopularQuestions,
    fetchDailyMessageCounts,
    fetchPlatformDistribution,
    fetchAll,
    clearError,
  }
})
