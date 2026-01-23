import apiClient from './client'
import type { ConversationDetail, ListConversationsResponse } from './types'

export interface ListConversationsParams {
  platform?: string
  search?: string
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}

/**
 * Conversations API endpoints
 */
export const conversationsApi = {
  /**
   * List all conversations (Admin only)
   */
  list: (params?: ListConversationsParams) =>
    apiClient.get<ListConversationsResponse>('/conversations', { params }),

  /**
   * Get conversation details with messages
   */
  getById: (id: string) => apiClient.get<ConversationDetail>(`/conversations/${id}`),

  /**
   * Export conversation to file
   */
  export: (id: string, format: 'json' | 'csv' = 'json') =>
    apiClient.get(`/conversations/${id}/export`, {
      params: { format },
      responseType: 'blob',
    }),

  /**
   * Delete a conversation
   */
  delete: (id: string) => apiClient.delete(`/conversations/${id}`),
}

export default conversationsApi
