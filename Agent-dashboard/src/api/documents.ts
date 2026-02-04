import apiClient from './client'
import type { DocumentDetail, ListDocumentsResponse } from './types'

export interface ListDocumentsParams {
  category?: string
  isProcessed?: boolean
  page?: number
  pageSize?: number
}

export interface UploadDocumentResponse {
  id: string
  fileName: string
  message: string
}

/**
 * Documents API endpoints
 */
export const documentsApi = {
  /**
   * List all documents with optional filtering
   */
  list: (params?: ListDocumentsParams) =>
    apiClient.get<ListDocumentsResponse>('/documents', { params }),

  /**
   * Get document details by ID
   */
  getById: (id: string) => apiClient.get<DocumentDetail>(`/documents/${id}`),

  /**
   * Upload a new document
   */
  upload: (file: File, title?: string, category?: string, description?: string) => {
    const formData = new FormData()
    formData.append('file', file)
    if (title) formData.append('title', title)
    if (category) formData.append('category', category)
    if (description) formData.append('description', description)

    return apiClient.post<UploadDocumentResponse>('/documents', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },

  /**
   * Delete a document
   */
  delete: (id: string) => apiClient.delete(`/documents/${id}`),

  /**
   * Get document content
   */
  getContent: (id: string) => apiClient.get<{ content: string }>(`/documents/${id}/content`),
}

export default documentsApi
