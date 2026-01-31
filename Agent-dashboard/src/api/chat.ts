import apiClient from './client'
import type { ConversationDetail, ListConversationsResponse } from './types'

export interface ListUserConversationsParams {
  sessionId?: string
  page?: number
  pageSize?: number
}

/**
 * Chat API endpoints for Users
 */
export const chatApi = {
  /**
   * List conversations for current user
   */
  list: (params?: ListUserConversationsParams) =>
    apiClient.get<ListConversationsResponse>('/chat/conversations', { params }),

  /**
   * Get conversation details for current user
   */
  getById: (id: string) => apiClient.get<ConversationDetail>(`/chat/conversations/${id}`),

  /**
   * Delete a conversation
   */
  delete: (id: string) => apiClient.delete(`/chat/conversations/${id}`),

  /**
   * Delete all conversations
   */
  deleteAll: () => apiClient.delete('/chat/conversations'),
}

export default chatApi
