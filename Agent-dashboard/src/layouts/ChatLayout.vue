<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { useConversationsStore } from '@/stores/conversations'
import AppIcon from '@/components/ui/AppIcon.vue'
import AppLogo from '@/components/ui/AppLogo.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'

const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const authStore = useAuthStore()
const conversationsStore = useConversationsStore()

const isSidebarOpen = ref(true)
const searchQuery = ref('')
let searchTimeout: any = null

// Modal State
const isConfirmModalOpen = ref(false)
const modalTitle = ref('')
const modalMessage = ref('')
const modalType = ref<'danger' | 'warning' | 'info'>('danger')
const confirmAction = ref<() => Promise<void>>(async () => {})
const isModalLoading = ref(false)

// Theme State
const currentTheme = ref<'light' | 'dark'>('light')

import { provide } from 'vue'
provide('isSidebarOpen', isSidebarOpen)
provide('toggleSidebar', toggleSidebar)

onMounted(async () => {
  // Theme initialization
  const savedTheme = localStorage.getItem('theme')
  if (savedTheme) {
    currentTheme.value = savedTheme as 'light' | 'dark'
    document.documentElement.setAttribute('data-theme', savedTheme)
  } else {
    // Check system preference
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      currentTheme.value = 'dark'
      document.documentElement.setAttribute('data-theme', 'dark')
    }
  }

  if (!authStore.isAuthenticated) {
    await authStore.initialize()
  }
  if (authStore.isAuthenticated) {
    await conversationsStore.fetchConversations()
  }

  // Auto-close on mobile initially
  if (window.innerWidth < 768) {
    isSidebarOpen.value = false
  }
})

function toggleTheme() {
  const newTheme = currentTheme.value === 'dark' ? 'light' : 'dark'
  currentTheme.value = newTheme
  document.documentElement.setAttribute('data-theme', newTheme)
  localStorage.setItem('theme', newTheme)
}

function toggleSidebar() {
  isSidebarOpen.value = !isSidebarOpen.value
}

function createNewChat() {
  router.push('/chat')
}

const handleSearch = () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    conversationsStore.fetchConversations({ search: searchQuery.value, page: 1 })
  }, 300)
}

let scrollTimeout: any = null
const isLoadingMore = ref(false)

const handleScroll = (e: Event) => {
  const target = e.target as HTMLElement
  if (target.scrollHeight - target.scrollTop <= target.clientHeight + 50) {
    // Debounce loadMore to prevent rapid calls and jitter
    if (scrollTimeout) clearTimeout(scrollTimeout)
    if (isLoadingMore.value || conversationsStore.isLoading) return

    scrollTimeout = setTimeout(async () => {
      isLoadingMore.value = true
      await conversationsStore.loadMore()
      isLoadingMore.value = false
    }, 150)
  }
}

// Open Modal Helper
function openConfirmModal(
  title: string,
  message: string,
  action: () => Promise<void>,
  type: 'danger' | 'warning' | 'info' = 'danger',
) {
  modalTitle.value = title
  modalMessage.value = message
  confirmAction.value = action
  modalType.value = type
  isConfirmModalOpen.value = true
}

async function executeConfirmAction() {
  isModalLoading.value = true
  try {
    await confirmAction.value()
    isConfirmModalOpen.value = false
  } finally {
    isModalLoading.value = false
  }
}

async function handleDeleteConversations(id: string) {
  openConfirmModal(
    'Delete Conversation',
    'Are you sure you want to delete this conversation? This action cannot be undone.',
    async () => {
      await conversationsStore.deleteConversation(id)
      const currentId = router.currentRoute.value.query.id
      if (currentId === id) {
        router.push('/chat')
      }
    },
  )
}

// Share conversation - copy link to clipboard
function handleShareConversation(id: string) {
  const shareUrl = `${window.location.origin}/chat?id=${id}`
  navigator.clipboard.writeText(shareUrl)
  // TODO: Show toast notification
  alert(t('chat.menu.shareCopied'))
}

// Pin conversation - placeholder for future implementation
function handlePinConversation(id: string) {
  // TODO: Implement pin functionality with API
  console.log('Pin conversation:', id)
  alert(t('chat.menu.pinSuccess'))
}

// Rename conversation
function handleRenameConversation(id: string, currentTitle: string) {
  const newTitle = prompt(t('chat.menu.renamePrompt'), currentTitle)
  if (newTitle && newTitle !== currentTitle) {
    // TODO: Implement rename API call
    console.log('Rename conversation:', id, 'to:', newTitle)
    alert(t('chat.menu.renameSuccess'))
  }
}

async function handleClearAllHistory() {
  openConfirmModal(
    'Clear All History',
    'Are you sure you want to delete ALL conversation history? This action cannot be undone.',
    async () => {
      await conversationsStore.deleteAllConversations()
      router.push('/chat')
    },
  )
}
</script>

<template>
  <div class="flex h-screen bg-base-100 overflow-hidden relative">
    <!-- Mobile Backdrop -->
    <div
      v-if="isSidebarOpen"
      @click="toggleSidebar"
      class="fixed inset-0 bg-base-content/20 backdrop-blur-sm z-20 md:hidden transition-opacity"
    ></div>

    <!-- Sidebar -->
    <aside
      class="flex-shrink-0 bg-base-200/80 backdrop-blur-xl border-r border-base-200 transition-all duration-300 flex flex-col fixed md:relative h-full z-40"
      :class="
        isSidebarOpen
          ? 'w-64 translate-x-0 md:shadow-none'
          : 'w-0 -translate-x-full opacity-0 overflow-hidden'
      "
    >
      <!-- Logo / Header -->
      <div
        class="h-14 flex items-center justify-between px-3 sticky top-0 bg-base-100/50 backdrop-blur-md z-20 border-b border-base-200/50"
      >
        <div class="flex items-center gap-2 font-bold text-base tracking-tight text-base-content">
          <div
            class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary"
          >
            <AppLogo class="w-5 h-5" />
          </div>
          <span>AI Agent</span>
        </div>
        <button
          class="btn btn-ghost btn-xs btn-square opacity-60 hover:opacity-100 transition-opacity"
          @click="toggleSidebar"
        >
          <AppIcon name="chevron-double-left" class="w-4 h-4" />
        </button>
      </div>

      <div class="p-3 space-y-3">
        <!-- New Chat Button -->
        <button
          @click="createNewChat"
          class="btn btn-primary btn-sm w-full gap-2 rounded-lg font-medium transition-all"
        >
          <AppIcon name="plus" class="w-4 h-4" />
          {{ t('actions.newChat') || 'New chat' }}
        </button>

        <!-- Search Bar -->
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="t('actions.search') || 'Search...'"
            class="input input-sm input-bordered w-full pl-9 rounded-lg bg-base-100/50 focus:bg-base-100 transition-colors text-xs"
            @input="handleSearch"
          />
          <AppIcon
            name="magnifying-glass"
            class="w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 text-base-content/40"
          />
        </div>
      </div>

      <!-- History List -->
      <div
        class="flex-1 overflow-y-auto px-2 pb-4 space-y-4 custom-scrollbar"
        @scroll="handleScroll"
      >
        <!-- Render grouped history here later -->
        <div
          v-if="conversationsStore.isLoading && conversationsStore.currentPage === 1"
          class="flex justify-center py-4"
        >
          <span class="loading loading-spinner loading-sm text-base-content/30"></span>
        </div>
        <div v-else class="space-y-4 pt-2">
          <div v-for="group in conversationsStore.groupedConversations" :key="group.label">
            <h3
              class="px-2 text-[11px] font-bold text-base-content/40 uppercase tracking-wider mb-2"
            >
              {{ group.label }}
            </h3>
            <div class="space-y-0.5">
              <div v-for="conv in group.items" :key="conv.id" class="group relative mx-2">
                <router-link
                  :to="`/chat?id=${conv.id}`"
                  :class="[
                    'block px-3 py-2 rounded-lg text-sm truncate transition-colors pr-8',
                    route.query.id === conv.id
                      ? 'bg-primary/10 text-primary font-medium'
                      : 'text-base-content/70 hover:bg-base-200 hover:text-base-content',
                  ]"
                >
                  {{
                    (conv as any).title ||
                    (conv as any).Title ||
                    conv.userName ||
                    'New Conversation'
                  }}
                </router-link>

                <!-- Simple Delete Button -->
                <button
                  @click.prevent="handleDeleteConversations(conv.id)"
                  class="absolute right-1 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 btn btn-ghost btn-xs btn-square text-base-content/50 hover:text-error h-6 w-6 min-h-0"
                  :title="$t('chat.menu.delete')"
                >
                  <AppIcon name="trash" class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          </div>

          <!-- Infinite Scroll Loader -->
          <div
            v-if="conversationsStore.isLoading && conversationsStore.currentPage > 1"
            class="flex justify-center py-2"
          >
            <span class="loading loading-spinner loading-xs text-base-content/30"></span>
          </div>
        </div>
      </div>

      <!-- User Profile Dropdown -->
      <div class="border-t border-base-200 bg-base-100/50">
        <div class="dropdown dropdown-top w-full">
          <div
            tabindex="0"
            role="button"
            class="flex items-center gap-3 p-3 mx-2 my-2 hover:bg-base-200 rounded-xl transition-colors cursor-pointer outline-none"
          >
            <div class="avatar placeholder">
              <div class="bg-primary/10 text-primary rounded-xl w-9 h-9">
                <span class="text-xs font-semibold">{{
                  authStore.user?.profile.name?.slice(0, 2).toUpperCase() || 'U'
                }}</span>
              </div>
            </div>
            <div class="flex-1 min-w-0 text-left">
              <p class="text-sm font-medium truncate">
                {{ authStore.user?.profile.name || 'User' }}
              </p>
              <p class="text-xs text-base-content/50 truncate">Pro Plan</p>
            </div>
            <AppIcon name="chevron-up" class="w-4 h-4 text-base-content/40" />
          </div>

          <ul
            tabindex="0"
            class="dropdown-content z-[1] p-1.5 bg-base-100 rounded-xl w-[calc(100%-1rem)] mx-2 mb-2 border border-base-200"
          >
            <li>
              <button
                @click="toggleTheme"
                class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-base-content/70 hover:bg-base-200 hover:text-base-content transition-colors"
              >
                <AppIcon v-if="currentTheme === 'dark'" name="sun" class="w-4 h-4" />
                <AppIcon v-else name="moon" class="w-4 h-4" />
                {{ currentTheme === 'dark' ? 'Light Mode' : 'Dark Mode' }}
              </button>
            </li>
            <li v-if="authStore.isAdmin">
              <router-link
                to="/dashboard"
                class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-primary hover:bg-primary/10 transition-colors"
              >
                <AppIcon name="home" class="w-4 h-4" />
                {{ t('sidebar.dashboard') }}
              </router-link>
            </li>
            <li v-if="conversationsStore.conversations.length > 0">
              <button
                @click="handleClearAllHistory"
                class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-error hover:bg-error/10 transition-colors"
              >
                <AppIcon name="trash" class="w-4 h-4" />
                Clear history
              </button>
            </li>
            <div class="h-px bg-base-200 my-1.5 mx-2"></div>
            <li>
              <button
                @click="authStore.logout()"
                class="flex items-center gap-3 w-full px-3 py-2.5 rounded-lg text-sm text-base-content/70 hover:bg-base-200 hover:text-base-content transition-colors"
              >
                <AppIcon name="logout" class="w-4 h-4" />
                Sign out
              </button>
            </li>
          </ul>
        </div>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="flex-1 flex flex-col min-w-0 h-full relative">
      <!-- Router View -->
      <router-view />
    </main>

    <!-- Confirm Modal -->
    <ConfirmModal
      :isOpen="isConfirmModalOpen"
      :title="modalTitle"
      :message="modalMessage"
      :type="modalType"
      :isLoading="isModalLoading"
      @close="isConfirmModalOpen = false"
      @confirm="executeConfirmAction"
    />
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: transparent;
  border-radius: 4px;
}
.custom-scrollbar:hover::-webkit-scrollbar-thumb {
  background: oklch(var(--bc) / 0.1);
}
</style>
