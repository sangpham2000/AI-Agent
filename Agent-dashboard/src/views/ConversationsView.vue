<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useConversationsStore } from '@/stores/conversations'
import type { ConversationSummary, ConversationDetail } from '@/api'

const conversationsStore = useConversationsStore()

// Filters
const searchQuery = ref('')
const platformFilter = ref('')
const dateFrom = ref('')
const dateTo = ref('')

// Pagination
const currentPage = ref(1)
const pageSize = ref(10)

// Selected conversation for detail view
const selectedConversation = ref<ConversationDetail | null>(null)
const isLoadingDetail = ref(false)

const filteredConversations = computed(() => {
  let result = conversationsStore.conversations

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(
      (c) =>
        c.userId?.toLowerCase().includes(query) ||
        c.userName?.toLowerCase().includes(query) ||
        c.userEmail?.toLowerCase().includes(query),
    )
  }

  if (platformFilter.value) {
    result = result.filter((c) => c.platform === platformFilter.value)
  }

  return result
})

const paginatedConversations = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredConversations.value.slice(start, start + pageSize.value)
})

const totalPages = computed(() => Math.ceil(filteredConversations.value.length / pageSize.value))

watch([searchQuery, platformFilter], () => {
  currentPage.value = 1
})

onMounted(async () => {
  await conversationsStore.fetchConversations()
})

async function viewConversation(conversation: ConversationSummary) {
  isLoadingDetail.value = true
  try {
    await conversationsStore.fetchConversationById(conversation.id)
    selectedConversation.value = conversationsStore.selectedConversation
  } finally {
    isLoadingDetail.value = false
  }
}

function closeDetail() {
  selectedConversation.value = null
  conversationsStore.setSelectedConversation(null)
}

async function handleExport() {
  await conversationsStore.exportConversations(
    platformFilter.value || undefined,
    dateFrom.value || undefined,
    dateTo.value || undefined,
  )
}

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">Conversation Logs</h1>
        <p class="text-sm text-base-content/50 mt-0.5">View and analyze conversation history.</p>
      </div>
      <button
        class="btn btn-ghost btn-sm gap-1.5 rounded-lg"
        @click="handleExport"
        :disabled="conversationsStore.isExporting"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-4 w-4"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          stroke-width="2"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
          />
        </svg>
        {{ conversationsStore.isExporting ? 'Exporting...' : 'Export' }}
      </button>
    </div>

    <!-- Alerts -->
    <div v-if="conversationsStore.error" class="alert alert-error text-sm py-3">
      <span>{{ conversationsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="conversationsStore.clearError()">Dismiss</button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1">Total Conversations</p>
        <p class="text-2xl font-bold">{{ conversationsStore.totalCount }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1">Total Messages</p>
        <p class="text-2xl font-bold">{{ conversationsStore.totalMessages }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1">Avg Messages/Conv</p>
        <p class="text-2xl font-bold">
          {{ conversationsStore.averageMessagesPerConversation.toFixed(1) }}
        </p>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-base-content/40"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search conversations..."
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-lg"
          />
        </div>
        <select
          v-model="platformFilter"
          class="select select-sm w-full sm:w-36 bg-base-200/50 border-0 rounded-lg"
        >
          <option value="">All Platforms</option>
          <option value="web">Web</option>
          <option value="telegram">Telegram</option>
          <option value="api">API</option>
        </select>
        <input
          v-model="dateFrom"
          type="date"
          class="input input-sm w-full sm:w-36 bg-base-200/50 border-0 rounded-lg"
          placeholder="From"
        />
        <input
          v-model="dateTo"
          type="date"
          class="input input-sm w-full sm:w-36 bg-base-200/50 border-0 rounded-lg"
          placeholder="To"
        />
      </div>
    </div>

    <!-- Conversations Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200">
              <th class="text-xs font-medium text-base-content/50">User / Session</th>
              <th class="text-xs font-medium text-base-content/50">Platform</th>
              <th class="text-xs font-medium text-base-content/50">Messages</th>
              <th class="text-xs font-medium text-base-content/50">Status</th>
              <th class="text-xs font-medium text-base-content/50">Started</th>
              <th class="text-xs font-medium text-base-content/50 w-16"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="conversationsStore.isLoading">
              <td colspan="6" class="text-center py-12">
                <span class="loading loading-spinner loading-md text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!paginatedConversations.length">
              <td colspan="6" class="text-center py-12 text-base-content/50">
                No conversations found
              </td>
            </tr>
            <tr
              v-for="conv in paginatedConversations"
              :key="conv.id"
              class="hover border-base-200 cursor-pointer"
              @click="viewConversation(conv)"
            >
              <td>
                <div class="flex items-center gap-2">
                  <div
                    class="w-8 h-8 rounded-lg bg-gradient-to-br from-primary/20 to-secondary/20 flex items-center justify-center"
                  >
                    <span class="text-xs font-semibold text-primary">{{
                      (conv.userName || conv.userId || 'U').slice(0, 2).toUpperCase()
                    }}</span>
                  </div>
                  <div>
                    <p class="font-medium text-sm">
                      {{ conv.userName || conv.userId || 'Unknown' }}
                    </p>
                    <p class="text-xs text-base-content/40">{{ conv.id.slice(0, 8) }}...</p>
                  </div>
                </div>
              </td>
              <td>
                <span
                  :class="[
                    'badge badge-sm border-0',
                    conv.platform === 'web'
                      ? 'bg-primary/10 text-primary'
                      : conv.platform === 'telegram'
                        ? 'bg-info/10 text-info'
                        : 'bg-secondary/10 text-secondary',
                  ]"
                >
                  {{ conv.platform }}
                </span>
              </td>
              <td class="text-sm">{{ conv.messageCount }}</td>
              <td>
                <span
                  :class="[
                    'badge badge-sm border-0',
                    conv.status === 'completed'
                      ? 'bg-success/10 text-success'
                      : conv.status === 'active'
                        ? 'bg-info/10 text-info'
                        : 'bg-warning/10 text-warning',
                  ]"
                >
                  {{ conv.status }}
                </span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatDate(conv.startedAt) }}</td>
              <td>
                <button class="btn btn-ghost btn-xs btn-square">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="h-4 w-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    stroke-width="1.75"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    />
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                    />
                  </svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div
        v-if="totalPages > 1"
        class="p-3 border-t border-base-200 flex items-center justify-between"
      >
        <p class="text-xs text-base-content/50">
          Showing {{ (currentPage - 1) * pageSize + 1 }} to
          {{ Math.min(currentPage * pageSize, filteredConversations.length) }} of
          {{ filteredConversations.length }}
        </p>
        <div class="join">
          <button class="join-item btn btn-xs" :disabled="currentPage === 1" @click="currentPage--">
            «
          </button>
          <button class="join-item btn btn-xs">Page {{ currentPage }}</button>
          <button
            class="join-item btn btn-xs"
            :disabled="currentPage === totalPages"
            @click="currentPage++"
          >
            »
          </button>
        </div>
      </div>
    </div>

    <!-- Conversation Detail Modal -->
    <dialog :class="['modal', { 'modal-open': selectedConversation }]">
      <div class="modal-box max-w-2xl max-h-[80vh]">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="font-semibold text-lg">Conversation Details</h3>
            <p class="text-xs text-base-content/50">{{ selectedConversation?.id }}</p>
          </div>
          <button class="btn btn-ghost btn-sm btn-square" @click="closeDetail">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2"
            >
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Loading -->
        <div v-if="isLoadingDetail" class="flex justify-center py-12">
          <span class="loading loading-spinner loading-md text-primary"></span>
        </div>

        <template v-else-if="selectedConversation">
          <!-- Info -->
          <div class="grid grid-cols-3 gap-3 mb-4">
            <div class="p-3 bg-base-200/50 rounded-xl">
              <p class="text-[10px] text-base-content/50 uppercase">User</p>
              <p class="text-sm font-medium">
                {{ selectedConversation.userName || selectedConversation.userId || 'Unknown' }}
              </p>
            </div>
            <div class="p-3 bg-base-200/50 rounded-xl">
              <p class="text-[10px] text-base-content/50 uppercase">Platform</p>
              <p class="text-sm font-medium capitalize">{{ selectedConversation.platform }}</p>
            </div>
            <div class="p-3 bg-base-200/50 rounded-xl">
              <p class="text-[10px] text-base-content/50 uppercase">Messages</p>
              <p class="text-sm font-medium">{{ selectedConversation.messages?.length || 0 }}</p>
            </div>
          </div>

          <!-- Messages -->
          <div class="overflow-y-auto max-h-96 space-y-3 pr-2">
            <div
              v-for="(message, idx) in selectedConversation.messages"
              :key="idx"
              :class="[
                'p-3 rounded-xl',
                message.role === 'user' ? 'bg-primary/10 ml-8' : 'bg-base-200 mr-8',
              ]"
            >
              <div class="flex items-center gap-2 mb-1">
                <span
                  :class="[
                    'text-xs font-semibold',
                    message.role === 'user' ? 'text-primary' : 'text-secondary',
                  ]"
                >
                  {{ message.role === 'user' ? 'User' : 'AI' }}
                </span>
                <span class="text-[10px] text-base-content/40">{{
                  formatDate(message.timestamp)
                }}</span>
              </div>
              <p class="text-sm whitespace-pre-wrap">{{ message.content }}</p>
            </div>
            <div
              v-if="!selectedConversation.messages?.length"
              class="text-center py-8 text-base-content/40 text-sm"
            >
              No messages in this conversation
            </div>
          </div>
        </template>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="closeDetail">close</button>
      </form>
    </dialog>
  </div>
</template>
