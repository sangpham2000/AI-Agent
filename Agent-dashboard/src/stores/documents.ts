import { defineStore } from 'pinia'
import { ref } from 'vue'
import { documentsApi } from '@/api'
import type { Document, DocumentDetail } from '@/api'

export const useDocumentsStore = defineStore('documents', () => {
  // State
  const documents = ref<Document[]>([])
  const selectedDocument = ref<DocumentDetail | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)
  const isLoading = ref(false)
  const isUploading = ref(false)
  const error = ref<string | null>(null)
  const successMessage = ref<string | null>(null)
  const uploadProgress = ref(0)

  // Actions
  async function fetchDocuments(params?: {
    category?: string
    isProcessed?: boolean
    page?: number
    pageSize?: number
  }) {
    isLoading.value = true
    error.value = null
    try {
      const response = await documentsApi.list(params)
      documents.value = response.data.items
      totalCount.value = response.data.totalCount
      currentPage.value = response.data.page
      pageSize.value = response.data.pageSize
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch documents'
    } finally {
      isLoading.value = false
    }
  }

  async function fetchDocumentById(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await documentsApi.getById(id)
      selectedDocument.value = response.data
      return response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch document'
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function fetchDocumentContent(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await documentsApi.getContent(id)
      return response.data.content
    } catch (e: any) {
      // Don't set global error for this, just return null so UI handles it
      console.error('Failed to fetch content', e)
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function uploadDocument(
    file: File,
    title?: string,
    category?: string,
    description?: string,
  ) {
    isUploading.value = true
    error.value = null
    successMessage.value = null
    uploadProgress.value = 0
    try {
      const response = await documentsApi.upload(file, title, category, description)
      successMessage.value = 'Document uploaded successfully'
      uploadProgress.value = 100
      await fetchDocuments()
      return response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to upload document'
      return null
    } finally {
      isUploading.value = false
    }
  }

  async function deleteDocument(id: string) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      await documentsApi.delete(id)
      documents.value = documents.value.filter((d) => d.id !== id)
      successMessage.value = 'Document deleted successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to delete document'
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearMessages() {
    error.value = null
    successMessage.value = null
  }

  function setSelectedDocument(doc: DocumentDetail | null) {
    selectedDocument.value = doc
  }

  return {
    documents,
    selectedDocument,
    totalCount,
    currentPage,
    pageSize,
    isLoading,
    isUploading,
    error,
    successMessage,
    uploadProgress,
    fetchDocuments,
    fetchDocumentById,
    fetchDocumentContent,
    uploadDocument,
    deleteDocument,
    clearMessages,
    setSelectedDocument,
  }
})
