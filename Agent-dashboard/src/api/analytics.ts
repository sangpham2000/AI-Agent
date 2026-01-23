import apiClient from './client'
import type {
  DashboardStats,
  ConversationTrend,
  PopularQuestion,
  DailyMessageCount,
  PlatformDistribution,
} from './types'

/**
 * Analytics API endpoints
 */
export const analyticsApi = {
  /**
   * Get dashboard statistics summary
   */
  getDashboardStats: () => apiClient.get<DashboardStats>('/analytics/dashboard'),

  /**
   * Get conversation trends over time
   */
  getConversationTrends: (days: number = 7) =>
    apiClient.get<ConversationTrend[]>('/analytics/conversations/trends', {
      params: { days },
    }),

  /**
   * Get popular questions
   */
  getPopularQuestions: (limit: number = 10) =>
    apiClient.get<PopularQuestion[]>('/analytics/questions/popular', {
      params: { limit },
    }),

  /**
   * Get daily message counts
   */
  getDailyMessageCounts: (days: number = 7) =>
    apiClient.get<DailyMessageCount[]>('/analytics/messages/daily', {
      params: { days },
    }),

  /**
   * Get platform distribution
   */
  getPlatformDistribution: () => apiClient.get<PlatformDistribution[]>('/analytics/platforms'),
}

export default analyticsApi
