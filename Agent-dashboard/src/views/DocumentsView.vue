<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useDocumentsStore } from '@/stores/documents'
import type { Document } from '@/api'
import AppIcon from '@/components/ui/AppIcon.vue'

const documentsStore = useDocumentsStore()

// Modal state
const showUploadModal = ref(false)
const showDeleteModal = ref(false)
const documentToDelete = ref<Document | null>(null)

// Upload form
const uploadFile = ref<File | null>(null)
const uploadTitle = ref('')
const uploadCategory = ref('')
const uploadDescription = ref('')
const fileInputRef = ref<HTMLInputElement | null>(null)

// Filters
const searchQuery = ref('')
const categoryFilter = ref('')
const processedFilter = ref<'all' | 'processed' | 'pending'>('all')

const categories = [
  'Admission',
  'Academic',
  'Student Affairs',
  'IT Support',
  'Regulations',
  'General',
]

const filteredDocuments = computed(() => {
  let result = documentsStore.documents

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(
      (d) =>
        d.title.toLowerCase().includes(query) ||
        d.fileName.toLowerCase().includes(query) ||
        d.category?.toLowerCase().includes(query),
    )
  }

  if (categoryFilter.value) {
    result = result.filter((d) => d.category === categoryFilter.value)
  }

  if (processedFilter.value === 'processed') {
    result = result.filter((d) => d.isProcessed)
  } else if (processedFilter.value === 'pending') {
    result = result.filter((d) => !d.isProcessed)
  }

  return result
})

onMounted(async () => {
  await documentsStore.fetchDocuments()
})

function handleFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files[0]) {
    uploadFile.value = target.files[0]
    if (!uploadTitle.value) {
      uploadTitle.value = target.files[0].name.replace(/\.[^/.]+$/, '')
    }
    // Reset input value to allow selecting the same file again
    target.value = ''
  }
}

function handleDrop(event: DragEvent) {
  event.preventDefault()
  if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
    uploadFile.value = event.dataTransfer.files[0]
    if (!uploadTitle.value) {
      uploadTitle.value = event.dataTransfer.files[0].name.replace(/\.[^/.]+$/, '')
    }
  }
}

function openUploadModal() {
  uploadFile.value = null
  uploadTitle.value = ''
  uploadCategory.value = ''
  uploadDescription.value = ''
  showUploadModal.value = true
}

function openDeleteModal(doc: Document) {
  documentToDelete.value = doc
  showDeleteModal.value = true
}

async function handleUpload() {
  if (!uploadFile.value) return

  const result = await documentsStore.uploadDocument(
    uploadFile.value,
    uploadTitle.value || undefined,
    uploadCategory.value || undefined,
    uploadDescription.value || undefined,
  )

  if (result) {
    showUploadModal.value = false
  }
}

async function handleDelete() {
  if (!documentToDelete.value) return
  const result = await documentsStore.deleteDocument(documentToDelete.value.id)
  if (result) {
    showDeleteModal.value = false
    documentToDelete.value = null
  }
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
}

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString()
}

function getFileIcon(fileType: string): string {
  if (fileType.includes('pdf')) return 'document-text'
  if (fileType.includes('word') || fileType.includes('doc')) return 'document-text'
  if (fileType.includes('text')) return 'document-text'
  return 'folder'
}
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">Document Management</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          Upload and manage knowledge base documents.
        </p>
      </div>
      <button class="btn btn-primary btn-sm gap-2 rounded-lg" @click="openUploadModal">
        <AppIcon name="upload" class="w-4 h-4" />
        Upload
      </button>
    </div>

    <!-- Alerts -->
    <div v-if="documentsStore.error" class="alert alert-error text-sm py-3 rounded-xl">
      <AppIcon name="exclamation" class="w-5 h-5" />
      <span>{{ documentsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="documentsStore.clearMessages()">Dismiss</button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="document-text" class="w-3.5 h-3.5" />
          Total Documents
        </p>
        <p class="text-2xl font-bold text-primary">{{ documentsStore.totalCount }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="check-circle" class="w-3.5 h-3.5" />
          Processed
        </p>
        <p class="text-2xl font-bold text-success">
          {{ documentsStore.documents.filter((d) => d.isProcessed).length }}
        </p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="clock" class="w-3.5 h-3.5" />
          Pending
        </p>
        <p class="text-2xl font-bold text-warning">
          {{ documentsStore.documents.filter((d) => !d.isProcessed).length }}
        </p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1 group">
          <AppIcon
            name="search"
            class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-base-content/40 group-focus-within:text-primary transition-colors"
          />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search documents..."
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none transition-all"
          />
        </div>
        <select
          v-model="categoryFilter"
          class="select select-sm w-full sm:w-36 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none"
        >
          <option value="">All Categories</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
        <select
          v-model="processedFilter"
          class="select select-sm w-full sm:w-32 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none"
        >
          <option value="all">All Status</option>
          <option value="processed">Processed</option>
          <option value="pending">Pending</option>
        </select>
      </div>
    </div>

    <!-- Documents Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200 bg-base-50/50">
              <th class="text-xs font-medium text-base-content/50">Document</th>
              <th class="text-xs font-medium text-base-content/50">Category</th>
              <th class="text-xs font-medium text-base-content/50">Size</th>
              <th class="text-xs font-medium text-base-content/50">Status</th>
              <th class="text-xs font-medium text-base-content/50">Uploaded</th>
              <th class="text-xs font-medium text-base-content/50 w-16"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="documentsStore.isLoading">
              <td colspan="6" class="text-center py-12">
                <span class="loading loading-spinner loading-md text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!filteredDocuments.length">
              <td colspan="6" class="text-center py-12 text-base-content/50">No documents found</td>
            </tr>
            <tr v-for="doc in filteredDocuments" :key="doc.id" class="hover border-base-100">
              <td>
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary"
                  >
                    <AppIcon :name="getFileIcon(doc.fileType)" class="w-5 h-5" />
                  </div>
                  <div>
                    <p class="font-medium text-sm">{{ doc.title }}</p>
                    <p class="text-xs text-base-content/50">{{ doc.fileName }}</p>
                  </div>
                </div>
              </td>
              <td>
                <span v-if="doc.category" class="badge badge-sm badge-ghost">{{
                  doc.category
                }}</span>
                <span v-else class="text-base-content/30">—</span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatFileSize(doc.fileSize) }}</td>
              <td>
                <span
                  :class="[
                    'badge badge-sm border-0',
                    doc.isProcessed ? 'bg-success/10 text-success' : 'bg-warning/10 text-warning',
                  ]"
                >
                  {{ doc.isProcessed ? 'Processed' : 'Pending' }}
                </span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatDate(doc.createdAt) }}</td>
              <td>
                <button
                  class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10 rounded-lg"
                  @click="openDeleteModal(doc)"
                >
                  <AppIcon name="trash" class="w-4 h-4" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Upload Modal -->
    <dialog :class="['modal', { 'modal-open': showUploadModal }]">
      <div class="modal-box max-w-lg rounded-2xl">
        <h3 class="font-semibold text-lg mb-4 flex items-center gap-2">
          <AppIcon name="upload" class="w-5 h-5 text-primary" />
          Upload Document
        </h3>
        <form @submit.prevent="handleUpload" class="space-y-4">
          <!-- Drop Zone -->
          <div
            class="border-2 border-dashed border-base-300 rounded-2xl p-8 text-center hover:border-primary/50 hover:bg-base-200/30 transition-all cursor-pointer group"
            @click="fileInputRef?.click()"
            @drop="handleDrop"
            @dragover.prevent
          >
            <input
              ref="fileInputRef"
              type="file"
              accept=".pdf,.doc,.docx,.txt,.md"
              class="hidden"
              @change="handleFileSelect"
            />
            <div v-if="uploadFile" class="space-y-2">
              <div
                class="w-12 h-12 mx-auto bg-primary/10 rounded-xl flex items-center justify-center text-primary"
              >
                <AppIcon :name="getFileIcon(uploadFile.type)" class="w-6 h-6" />
              </div>
              <p class="font-medium text-sm">{{ uploadFile.name }}</p>
              <p class="text-xs text-base-content/50">{{ formatFileSize(uploadFile.size) }}</p>
            </div>
            <div v-else class="space-y-2">
              <div
                class="w-12 h-12 mx-auto bg-base-200 rounded-xl flex items-center justify-center text-base-content/30 group-hover:text-primary group-hover:scale-110 transition-all"
              >
                <AppIcon name="cloud-upload" class="w-6 h-6" />
              </div>
              <p class="text-sm text-base-content/60">Drop files here or click to upload</p>
              <p class="text-xs text-base-content/40">PDF, DOC, DOCX, TXT, MD</p>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Title</label>
              <input
                v-model="uploadTitle"
                type="text"
                class="input input-sm input-bordered rounded-lg focus:outline-none focus:border-primary"
                placeholder="Document title"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                Category <span class="text-error">*</span>
              </label>
              <select
                v-model="uploadCategory"
                class="select select-sm select-bordered rounded-lg focus:outline-none focus:border-primary"
              >
                <option value="">Select...</option>
                <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
              </select>
            </div>
          </div>
          <div v-if="documentsStore.isUploading" class="space-y-1">
            <progress
              class="progress progress-primary w-full h-2"
              :value="documentsStore.uploadProgress"
              max="100"
            ></progress>
            <p class="text-xs text-center text-base-content/50">
              Uploading... {{ documentsStore.uploadProgress }}%
            </p>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              class="btn btn-ghost btn-sm rounded-lg"
              @click="showUploadModal = false"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="btn btn-primary btn-sm rounded-lg"
              :disabled="!uploadFile || !uploadCategory || documentsStore.isUploading"
            >
              <span
                v-if="documentsStore.isUploading"
                class="loading loading-spinner loading-xs"
              ></span>
              Upload
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showUploadModal = false">close</button>
      </form>
    </dialog>

    <!-- Delete Modal -->
    <dialog :class="['modal', { 'modal-open': showDeleteModal }]">
      <div class="modal-box max-w-sm rounded-2xl">
        <h3 class="font-semibold text-lg flex items-center gap-2 text-error">
          <AppIcon name="trash" class="w-5 h-5" />
          Delete Document
        </h3>
        <p class="py-4 text-sm text-base-content/70">
          Are you sure you want to delete <strong>{{ documentToDelete?.title }}</strong
          >? All processed chunks will also be removed.
        </p>
        <div class="flex justify-end gap-2">
          <button class="btn btn-ghost btn-sm rounded-lg" @click="showDeleteModal = false">
            Cancel
          </button>
          <button
            class="btn btn-error btn-sm rounded-lg"
            @click="handleDelete"
            :disabled="documentsStore.isLoading"
          >
            Delete
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showDeleteModal = false">close</button>
      </form>
    </dialog>
  </div>
</template>
