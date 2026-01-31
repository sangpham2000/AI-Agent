<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConversationsStore } from '@/stores/conversations'
import AppIcon from '@/components/ui/AppIcon.vue'
import ConfirmModal from '@/components/ui/ConfirmModal.vue'

const router = useRouter()
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

const handleScroll = (e: Event) => {
  const target = e.target as HTMLElement
  if (target.scrollHeight - target.scrollTop <= target.clientHeight + 50) {
    conversationsStore.loadMore()
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
  <div class="flex h-screen bg-base-100 overflow-hidden">
    <!-- Sidebar -->
    <aside
      class="flex-shrink-0 bg-base-200/50 border-r border-base-200 transition-all duration-300 flex flex-col"
      :class="
        isSidebarOpen ? 'w-64 translate-x-0' : 'w-0 -translate-x-full opacity-0 overflow-hidden'
      "
    >
      <!-- Logo / Header -->
      <div class="h-16 flex items-center justify-between px-4 sticky top-0 bg-base-100 z-20">
        <div class="flex items-center gap-2.5 font-bold text-lg tracking-tight text-base-content">
          <div
            class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary"
          >
            <AppIcon name="sparkles" class="w-5 h-5" />
          </div>
          <span>AI Agent</span>
        </div>
        <button
          class="btn btn-ghost btn-xs btn-square opacity-50 hover:opacity-100 transition-opacity"
          @click="toggleSidebar"
        >
          <AppIcon name="chevron-double-left" class="w-4 h-4" />
        </button>
      </div>

      <!-- New Chat Button -->
      <div class="px-3 mb-6">
        <button
          @click="createNewChat"
          class="btn btn-primary w-full justify-start gap-3 rounded-xl shadow-lg shadow-primary/20 hover:shadow-primary/30 transition-all border-0 bg-gradient-to-r from-primary to-primary/80 text-white font-medium"
        >
          <AppIcon name="plus" class="w-5 h-5" />
          New chat
        </button>
      </div>

      <!-- Search Bar -->
      <div class="px-3 mb-2">
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search conversations..."
            class="input input-sm input-bordered w-full pl-9 rounded-lg bg-base-100 focus:bg-base-100 transition-colors"
            @input="handleSearch"
          />
          <AppIcon
            name="magnifying-glass"
            class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-base-content/40"
          />
        </div>
      </div>

      <!-- History List -->
      <div
        class="flex-1 overflow-y-auto px-3 pb-4 space-y-4 custom-scrollbar"
        @scroll="handleScroll"
      >
        <!-- Render grouped history here later -->
        <div
          v-if="conversationsStore.isLoading && conversationsStore.currentPage === 1"
          class="flex justify-center py-4"
        >
          <span class="loading loading-spinner loading-sm text-base-content/30"></span>
        </div>
        <div v-else class="space-y-4">
          <div v-for="group in conversationsStore.groupedConversations" :key="group.label">
            <h3
              class="px-3 text-xs font-semibold text-base-content/40 uppercase tracking-wider mb-2 sticky top-0 bg-base-100/90 backdrop-blur-sm py-1 z-10"
            >
              {{ group.label }}
            </h3>
            <div class="space-y-0.5">
              <div v-for="conv in group.items" :key="conv.id" class="group relative mx-2">
                <router-link
                  :to="`/chat?id=${conv.id}`"
                  class="block px-3 py-2 rounded-lg hover:bg-base-200 text-sm truncate text-base-content/70 hover:text-base-content transition-colors pr-8"
                  active-class="bg-base-200 text-base-content font-medium"
                >
                  {{
                    (conv as any).title ||
                    (conv as any).Title ||
                    conv.userName ||
                    'New Conversation'
                  }}
                </router-link>
                <button
                  @click.prevent="handleDeleteConversations(conv.id)"
                  class="absolute right-1 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 btn btn-ghost btn-xs btn-square text-error h-6 w-6 min-h-0"
                  title="Delete conversation"
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
            class="flex items-center gap-3 p-4 hover:bg-base-200 transition-colors cursor-pointer outline-none"
          >
            <div class="avatar placeholder">
              <div class="bg-neutral text-neutral-content rounded-full w-8">
                <span class="text-xs">{{
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
            class="dropdown-content z-[1] menu p-2 shadow-xl bg-base-100 rounded-xl w-[calc(100%-1rem)] mx-2 mb-2 border border-base-200"
          >
            <li>
              <button @click="toggleTheme" class="flex gap-3">
                <AppIcon v-if="currentTheme === 'dark'" name="sun" class="w-4 h-4" />
                <AppIcon v-else name="moon" class="w-4 h-4" />
                {{ currentTheme === 'dark' ? 'Light Mode' : 'Dark Mode' }}
              </button>
            </li>
            <li v-if="conversationsStore.conversations.length > 0">
              <button
                @click="handleClearAllHistory"
                class="flex gap-3 text-error hover:bg-error/10"
              >
                <AppIcon name="trash" class="w-4 h-4" />
                Clear history
              </button>
            </li>
            <div class="divider my-1"></div>
            <li>
              <button
                @click="authStore.logout()"
                class="flex gap-3 text-base-content/70 hover:bg-base-200"
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
      <!-- Toggle Button (Visible when sidebar closed) -->
      <button
        v-if="!isSidebarOpen"
        @click="toggleSidebar"
        class="absolute top-4 left-4 z-50 btn btn-circle btn-sm btn-ghost bg-base-100 shadow-md border border-base-200"
      >
        <AppIcon name="chevron-double-right" class="w-4 h-4" />
      </button>

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
