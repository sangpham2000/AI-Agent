<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useDocumentsStore } from '@/stores/documents'
import type { Document } from '@/api'
import AppIcon from '@/components/ui/AppIcon.vue'
import { useI18n } from 'vue-i18n'
import { formatDate } from '@/utils/format'

const { t } = useI18n()
const documentsStore = useDocumentsStore()

// Modal state
const showUploadModal = ref(false)
const showDeleteModal = ref(false)
const showPreviewModal = ref(false)
const documentToDelete = ref<Document | null>(null)
const previewContent = ref<string>('')
const previewTitle = ref<string>('')

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
  'Academic Resources',
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

async function openPreviewModal(doc: Document) {
  previewTitle.value = doc.title
  previewContent.value = 'Loading...'
  showPreviewModal.value = true

  const content = await documentsStore.fetchDocumentContent(doc.id)
  previewContent.value = content || 'Preview not available or file is empty.'
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
        <h1 class="text-xl font-semibold">{{ t('documents.title') }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          {{ t('documents.subtitle') }}
        </p>
      </div>
      <button class="btn btn-primary btn-sm gap-2 rounded-lg" @click="openUploadModal">
        <AppIcon name="upload" class="w-4 h-4" />
        {{ t('actions.upload') }}
      </button>
    </div>

    <!-- Alerts -->
    <div v-if="documentsStore.error" class="alert alert-error text-sm py-3 rounded-xl">
      <AppIcon name="exclamation" class="w-5 h-5" />
      <span>{{ documentsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="documentsStore.clearMessages()">
        {{ t('actions.dismiss') }}
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="document-text" class="w-3.5 h-3.5" />
          {{ t('documents.totalDocuments') }}
        </p>
        <p class="text-2xl font-bold text-primary">{{ documentsStore.totalCount }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="check-circle" class="w-3.5 h-3.5" />
          {{ t('documents.processed') }}
        </p>
        <p class="text-2xl font-bold text-success">
          {{ documentsStore.documents.filter((d) => d.isProcessed).length }}
        </p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="clock" class="w-3.5 h-3.5" />
          {{ t('documents.pending') }}
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
            :placeholder="t('documents.searchPlaceholder')"
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none transition-all"
          />
        </div>
        <select
          v-model="categoryFilter"
          class="select select-sm w-full sm:w-36 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none"
        >
          <option value="">{{ t('documents.allCategories') }}</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
        <select
          v-model="processedFilter"
          class="select select-sm w-full sm:w-32 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none"
        >
          <option value="all">{{ t('documents.allStatus') }}</option>
          <option value="processed">{{ t('documents.processed') }}</option>
          <option value="pending">{{ t('documents.pending') }}</option>
        </select>
      </div>
    </div>

    <!-- Documents Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200 bg-base-50/50">
              <th class="text-xs font-medium text-base-content/50">
                {{ t('documents.table.document') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('documents.table.category') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('documents.table.size') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('documents.table.status') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('documents.table.uploaded') }}
              </th>
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
              <td colspan="6" class="text-center py-12 text-base-content/50">
                {{ t('documents.table.noDocuments') }}
              </td>
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
                  {{ doc.isProcessed ? t('documents.processed') : t('documents.pending') }}
                </span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatDate(doc.createdAt) }}</td>
              <td>
                <div class="flex items-center gap-1">
                  <button
                    class="btn btn-ghost btn-xs btn-square text-base-content/70 hover:text-primary hover:bg-primary/10 rounded-lg"
                    @click="openPreviewModal(doc)"
                    :title="t('actions.view')"
                  >
                    <AppIcon name="eye" class="w-4 h-4" />
                  </button>
                  <button
                    class="btn btn-ghost btn-xs btn-square text-error/70 hover:text-error hover:bg-error/10 rounded-lg"
                    @click="openDeleteModal(doc)"
                    :title="t('actions.delete')"
                  >
                    <AppIcon name="trash" class="w-4 h-4" />
                  </button>
                </div>
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
          {{ t('documents.uploadTitle') }}
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
              <p class="text-sm text-base-content/60">{{ t('documents.dragDrop') }}</p>
              <p class="text-xs text-base-content/40">{{ t('documents.supportedFormats') }}</p>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">{{
                t('documents.form.title')
              }}</label>
              <input
                v-model="uploadTitle"
                type="text"
                class="input input-sm input-bordered rounded-lg focus:outline-none focus:border-primary"
                :placeholder="t('documents.form.title')"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('documents.form.category') }} <span class="text-error">*</span>
              </label>
              <select
                v-model="uploadCategory"
                class="select select-sm select-bordered rounded-lg focus:outline-none focus:border-primary"
              >
                <option value="">{{ t('documents.form.selectCategory') }}</option>
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
              {{ t('documents.form.uploading') }} {{ documentsStore.uploadProgress }}%
            </p>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              class="btn btn-ghost btn-sm rounded-lg"
              @click="showUploadModal = false"
            >
              {{ t('actions.cancel') }}
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
              {{ t('actions.upload') }}
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
          {{ t('documents.deleteModal.title') }}
        </h3>
        <p
          class="py-4 text-sm text-base-content/70"
          v-html="t('documents.deleteModal.message', { title: documentToDelete?.title })"
        ></p>
        <div class="flex justify-end gap-2">
          <button class="btn btn-ghost btn-sm rounded-lg" @click="showDeleteModal = false">
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-error btn-sm rounded-lg"
            @click="handleDelete"
            :disabled="documentsStore.isLoading"
          >
            {{ t('actions.delete') }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showDeleteModal = false">close</button>
      </form>
    </dialog>

    <!-- Preview Modal -->
    <dialog :class="['modal', { 'modal-open': showPreviewModal }]">
      <div class="modal-box w-11/12 max-w-4xl h-[80vh] rounded-2xl flex flex-col p-0">
        <div class="flex items-center justify-between p-4 border-b border-base-200">
          <h3 class="font-semibold text-lg flex items-center gap-2">
            <AppIcon name="document-text" class="w-5 h-5 text-primary" />
            {{ previewTitle }}
          </h3>
          <button
            class="btn btn-ghost btn-sm btn-square rounded-lg"
            @click="showPreviewModal = false"
          >
            <AppIcon name="x" class="w-5 h-5" />
          </button>
        </div>
        <div class="flex-1 overflow-y-auto p-6 bg-base-200/50">
          <div
            class="bg-base-100 rounded-xl p-6 shadow-sm min-h-full font-mono text-sm whitespace-pre-wrap leading-relaxed"
          >
            {{ previewContent }}
          </div>
        </div>
        <div class="p-4 border-t border-base-200 flex justify-end">
          <button class="btn btn-primary btn-sm rounded-lg" @click="showPreviewModal = false">
            {{ t('actions.close') }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showPreviewModal = false">close</button>
      </form>
    </dialog>
  </div>
</template>
