<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useConversationsStore } from '@/stores/conversations'
import type { ConversationSummary, ConversationDetail } from '@/api'
import MarkdownIt from 'markdown-it'
import AppIcon from '@/components/ui/AppIcon.vue'

const { t } = useI18n()
const md = new MarkdownIt({
  html: false,
  breaks: true,
  linkify: true,
})

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

function formatDate(dateString?: string) {
  if (!dateString) return ''
  return new Date(dateString).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function renderMessageContent(content: string) {
  if (!content) return ''
  return md.render(content)
}
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">{{ t('conversations.title') }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">{{ t('conversations.subtitle') }}</p>
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
        {{
          conversationsStore.isExporting ? t('conversations.exporting') : t('conversations.export')
        }}
      </button>
    </div>

    <!-- Alerts -->
    <div
      v-if="conversationsStore.error"
      class="alert alert-soft alert-error text-sm py-3 rounded-xl"
    >
      <span>{{ conversationsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="conversationsStore.clearError()">
        {{ t('actions.dismiss') }}
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chat" class="w-3.5 h-3.5" />
          {{ t('conversations.totalConversations') }}
        </p>
        <p class="text-2xl font-bold">{{ conversationsStore.totalCount }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="envelope" class="w-3.5 h-3.5" />
          {{ t('conversations.totalMessages') }}
        </p>
        <p class="text-2xl font-bold">{{ conversationsStore.totalMessages }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chart-bar" class="w-3.5 h-3.5" />
          {{ t('conversations.avgMessages') }}
        </p>
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
            :placeholder="t('conversations.searchPlaceholder')"
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none transition-all"
          />
        </div>
        <select
          v-model="platformFilter"
          class="select select-sm w-full sm:w-36 bg-base-100 border border-base-200 rounded-xl focus:border-primary focus:outline-none"
        >
          <option value="">{{ t('conversations.allPlatforms') }}</option>
          <option value="web">Web</option>
          <option value="telegram">Telegram</option>
          <option value="api">API</option>
        </select>
        <input
          v-model="dateFrom"
          type="date"
          class="input input-sm w-full sm:w-36 bg-base-100 border border-base-200 rounded-xl focus:border-primary focus:outline-none"
          :placeholder="t('conversations.from')"
        />
        <input
          v-model="dateTo"
          type="date"
          class="input input-sm w-full sm:w-36 bg-base-100 border border-base-200 rounded-xl focus:border-primary focus:outline-none"
          :placeholder="t('conversations.to')"
        />
      </div>
    </div>

    <!-- Conversations Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200">
              <th class="text-xs font-medium text-base-content/50">
                {{ t('conversations.table.userSession') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('conversations.table.platform') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('conversations.table.messages') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('conversations.table.status') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('conversations.table.started') }}
              </th>
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
                {{ t('common.noData') }}
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
                <div class="flex items-center gap-1">
                  <button
                    class="btn btn-ghost btn-xs btn-square text-base-content/70 hover:text-primary hover:bg-primary/10 rounded-lg"
                  >
                    <AppIcon name="eye" class="w-4 h-4" />
                  </button>
                </div>
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
          {{ t('common.showing') }} {{ (currentPage - 1) * pageSize + 1 }}
          {{ t('common.to') }}
          {{ Math.min(currentPage * pageSize, filteredConversations.length) }}
          {{ t('common.of') }}
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

    <!-- Conversation Detail Drawer -->
    <div class="z-50 relative" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
      <!-- Backdrop -->
      <div
        v-if="selectedConversation"
        class="fixed inset-0 bg-black/20 backdrop-blur-sm transition-opacity duration-300 ease-in-out"
        @click="closeDetail"
      ></div>

      <!-- Slide-over panel -->
      <div class="fixed inset-0 overflow-hidden pointer-events-none">
        <div class="absolute inset-0 overflow-hidden">
          <div class="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
            <div
              class="pointer-events-auto w-screen max-w-2xl transform transition duration-300 ease-in-out sm:duration-500 bg-base-100 shadow-2xl flex flex-col h-full border-l border-base-200"
              :class="selectedConversation ? 'translate-x-0' : 'translate-x-full'"
            >
              <!-- Header -->
              <div
                class="flex-shrink-0 px-6 py-4 flex items-center justify-between border-b border-base-200 bg-base-100/80 backdrop-blur-md sticky top-0 z-10"
              >
                <div>
                  <h2 class="text-lg font-semibold text-base-content">
                    {{ selectedConversation?.title || t('conversations.detail.title') }}
                  </h2>
                  <div class="flex items-center gap-2 mt-1">
                    <span class="text-xs text-base-content/50"
                      >{{ selectedConversation?.id?.split('-')[0] }}...</span
                    >
                    <span class="badge badge-xs badge-ghost capitalize">{{
                      selectedConversation?.platform
                    }}</span>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <button class="btn btn-ghost btn-sm btn-square" @click="closeDetail">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-5 w-5"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>
              </div>

              <!-- Content -->
              <div class="flex-1 overflow-y-auto p-6 relative">
                <!-- Loading -->
                <div
                  v-if="isLoadingDetail"
                  class="absolute inset-0 flex items-center justify-center bg-base-100/50 z-10"
                >
                  <span class="loading loading-spinner loading-lg text-primary"></span>
                </div>

                <template v-else-if="selectedConversation">
                  <!-- User Info Card -->
                  <div class="bg-base-100 rounded-2xl border border-base-200 p-5 mb-6">
                    <div class="flex items-center gap-4">
                      <!-- Avatar -->
                      <div
                        class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary font-bold text-lg"
                      >
                        {{ (selectedConversation.userName || 'U').charAt(0).toUpperCase() }}
                      </div>

                      <!-- User Info -->
                      <div class="flex-1 min-w-0">
                        <p class="font-semibold truncate">
                          {{ selectedConversation.userName || 'Unknown User' }}
                        </p>
                        <p class="text-sm text-base-content/50 truncate">
                          {{ selectedConversation.userEmail || 'No email' }}
                        </p>
                      </div>

                      <!-- Stats -->
                      <div class="flex items-center gap-3">
                        <div class="text-center px-3 py-2 bg-base-200/50 rounded-xl">
                          <p class="text-lg font-bold text-primary">
                            {{ selectedConversation.messages?.length || 0 }}
                          </p>
                          <p class="text-[10px] text-base-content/50 uppercase font-medium">
                            Messages
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Chat History -->
                  <div class="space-y-6 pb-4">
                    <div
                      v-for="(message, idx) in selectedConversation.messages"
                      :key="idx"
                      class="group w-full max-w-3xl mx-auto"
                    >
                      <div
                        class="flex gap-4 p-2 rounded-xl transition-colors hover:bg-base-200/50"
                        :class="message.role === 'user' ? 'flex-row-reverse' : 'flex-row'"
                      >
                        <!-- Avatar -->
                        <div
                          class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center mt-0.5"
                          :class="
                            message.role === 'user'
                              ? 'bg-base-200'
                              : 'bg-transparent border border-base-200'
                          "
                        >
                          <span
                            v-if="message.role === 'user'"
                            class="text-[10px] font-medium text-base-content/60"
                            >{{ t('conversations.detail.user') }}</span
                          >
                          <AppIcon v-else name="sparkles" class="w-4 h-4 text-base-content" />
                        </div>

                        <!-- Content -->
                        <div
                          class="flex flex-col gap-1 min-w-0 max-w-[85%]"
                          :class="message.role === 'user' ? 'items-end' : 'items-start'"
                        >
                          <div class="flex items-center gap-2 px-1">
                            <span class="text-xs font-medium text-base-content">
                              {{
                                message.role === 'user' ? 'User' : t('conversations.detail.user')
                              }}
                            </span>
                          </div>

                          <div
                            class="text-sm leading-7"
                            :class="[
                              message.role === 'user'
                                ? 'bg-base-200 px-4 py-2 rounded-2xl rounded-tr-sm text-base-content'
                                : 'text-base-content px-1',
                            ]"
                          >
                            <!-- Markdown Content -->
                            <div
                              v-if="message.role !== 'user'"
                              class="markdown-body prose prose-sm max-w-none prose-p:my-1 prose-headings:my-2 prose-code:text-base-content prose-code:bg-base-200 prose-code:px-1 prose-code:rounded prose-code:font-normal prose-pre:bg-base-300 prose-pre:text-base-content"
                              v-html="renderMessageContent(message.content)"
                            ></div>
                            <p v-else class="whitespace-pre-wrap">{{ message.content }}</p>
                          </div>

                          <!-- Timestamp -->
                          <div class="px-1">
                            <time class="text-[10px] opacity-40">{{
                              formatDate(message.createdAt)
                            }}</time>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div
                      v-if="!selectedConversation.messages?.length"
                      class="text-center py-12 text-base-content/40 italic"
                    >
                      {{ t('conversations.detail.noMessages') }}
                    </div>
                  </div>
                </template>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
/* Markdown Styles Override using DaisyUI Semantic Colors (Matched with ChatView) */
:deep(.markdown-body) {
  font-size: 0.925rem;
  line-height: 1.6;
  color: oklch(var(--bc));
}
:deep(.markdown-body ul) {
  list-style-type: disc;
  padding-left: 1.5em;
}
:deep(.markdown-body ol) {
  list-style-type: decimal;
  padding-left: 1.5em;
}
:deep(.markdown-body strong) {
  font-weight: 600;
  color: inherit;
}
:deep(.markdown-body a) {
  color: oklch(var(--p));
  text-decoration: underline;
  text-underline-offset: 2px;
}
:deep(.markdown-body blockquote) {
  border-left: 4px solid oklch(var(--nc));
  color: oklch(var(--bc) / 0.6);
  padding-left: 1em;
}
</style>
