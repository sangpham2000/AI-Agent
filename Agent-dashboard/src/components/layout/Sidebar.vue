<script setup lang="ts">
import { watch, computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import AppLogo from '@/components/ui/AppLogo.vue'

const props = defineProps<{
  isCollapsed: boolean
  isMobileOpen: boolean
}>()

const emit = defineEmits<{
  toggleCollapse: []
  closeMobile: []
}>()

const { t } = useI18n()

const route = useRoute()
const authStore = useAuthStore()

// Close mobile menu on route change
watch(
  () => route.path,
  () => {
    emit('closeMobile')
  },
)

interface MenuItem {
  name: string
  path: string
  icon: string
  badge?: string
}

interface MenuGroup {
  id: string
  label: string
  items: MenuItem[]
}

const menuGroups = computed(() => {
  const allMenuGroups: MenuGroup[] = [
    {
      id: 'overview',
      label: t('sidebar.overview'),
      items: [
        { name: t('sidebar.dashboard'), path: '/dashboard', icon: 'home' },
        { name: t('sidebar.analytics'), path: '/dashboard/analytics', icon: 'chart' },
      ],
    },
    {
      id: 'management',
      label: t('sidebar.management'),
      items: [
        { name: t('sidebar.users'), path: '/dashboard/users', icon: 'users' },
        { name: t('sidebar.roles'), path: '/dashboard/roles', icon: 'lock' },
        { name: t('sidebar.documents'), path: '/dashboard/documents', icon: 'document' },
        { name: t('sidebar.conversations'), path: '/dashboard/conversations', icon: 'chat' },
      ],
    },
    {
      id: 'tools',
      label: t('sidebar.tools'),
      items: [{ name: t('sidebar.chatDemo'), path: '/chat', icon: 'message', badge: 'Live' }],
    },
  ]

  if (authStore.isAdmin) return allMenuGroups
  // Non-admin only sees Tools (Chat)
  return allMenuGroups.filter((g) => g.id === 'tools')
})

const isActive = (path: string) => {
  if (path === '/dashboard') return route.path === '/dashboard'
  return route.path.startsWith(path)
}

// Icon paths
const icons: Record<string, string> = {
  home: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
  chart:
    'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
  users:
    'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
  document:
    'M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z',
  chat: 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z',
  message:
    'M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z',
  lock: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z',
  logout:
    'M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1',
}
</script>

<template>
  <!-- Mobile Overlay -->
  <div
    v-if="isMobileOpen"
    class="fixed inset-0 bg-black/40 z-40 lg:hidden backdrop-blur-sm"
    @click="emit('closeMobile')"
  ></div>

  <!-- Sidebar -->
  <aside
    :class="[
      'fixed left-0 top-0 z-50 h-screen bg-base-100 flex flex-col transition-all duration-200 ease-out border-r border-base-200',
      isCollapsed ? 'w-16' : 'w-56',
      isMobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
    ]"
  >
    <!-- Logo -->
    <div class="h-16 flex items-center px-4 border-b border-base-200 flex-shrink-0">
      <RouterLink to="/" class="flex items-center gap-2.5">
        <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center flex-shrink-0">
          <AppLogo class="h-4 w-4 text-primary-content" />
        </div>
        <span v-if="!isCollapsed" class="font-semibold text-base">SA AI Dashboard</span>
      </RouterLink>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 overflow-y-auto py-3 px-2.5">
      <template v-for="group in menuGroups" :key="group.id">
        <!-- Group Label (static, not clickable) -->
        <p
          v-if="!isCollapsed"
          class="px-2 py-1.5 mt-3 first:mt-0 text-[10px] font-semibold text-base-content/40 uppercase tracking-wider"
        >
          {{ group.label }}
        </p>

        <!-- Divider for collapsed state -->
        <div
          v-if="isCollapsed && group.id !== 'overview'"
          class="my-2 mx-2 border-t border-base-200"
        ></div>

        <!-- Group Items (always visible) -->
        <div class="space-y-0.5 mt-1">
          <RouterLink
            v-for="item in group.items"
            :key="item.path"
            :to="item.path"
            :title="isCollapsed ? item.name : undefined"
            :class="[
              'group flex items-center gap-2.5 px-2.5 py-2 rounded-lg text-[13px] font-medium transition-all duration-150',
              isCollapsed ? 'justify-center' : '',
              isActive(item.path)
                ? 'bg-primary/10 text-primary'
                : 'text-base-content/70 hover:bg-base-200 hover:text-base-content',
            ]"
          >
            <div class="w-5 h-5 flex items-center justify-center flex-shrink-0">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-[18px] w-[18px]"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="1.75"
              >
                <path stroke-linecap="round" stroke-linejoin="round" :d="icons[item.icon]" />
              </svg>
            </div>
            <span v-if="!isCollapsed" class="flex-1">{{ item.name }}</span>
            <span
              v-if="!isCollapsed && item.badge"
              class="px-1.5 py-0.5 text-[10px] font-semibold rounded bg-success/15 text-success"
            >
              {{ item.badge }}
            </span>
          </RouterLink>
        </div>
      </template>
    </nav>

    <!-- Footer -->
    <div class="p-2.5 border-t border-base-200 flex-shrink-0">
      <!-- User -->
      <!-- <div
        :class="[
          'flex items-center gap-2.5 p-2 rounded-lg hover:bg-base-200 transition-colors cursor-pointer',
          isCollapsed ? 'justify-center' : '',
        ]"
      >
        <div class="avatar">
          <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-primary to-secondary">
            <span class="flex items-center justify-center h-full text-xs font-semibold text-white">
              {{ authStore.userName?.[0]?.toUpperCase() || 'A' }}
            </span>
          </div>
        </div>
        <div v-if="!isCollapsed" class="flex-1 min-w-0">
          <p class="text-sm font-medium truncate">{{ authStore.userName || 'Admin' }}</p>
          <p class="text-[11px] text-base-content/50 truncate">
            {{ authStore.isAdmin ? 'Administrator' : 'User' }}
          </p>
        </div>
        <button
          v-if="!isCollapsed"
          @click.stop="authStore.logout()"
          class="p-1 rounded hover:bg-base-300 transition-colors"
          title="Logout"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-4 w-4 text-base-content/50"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="1.75"
          >
            <path stroke-linecap="round" stroke-linejoin="round" :d="icons.logout" />
          </svg>
        </button>
      </div> -->

      <!-- Collapse Toggle -->
      <button
        @click="emit('toggleCollapse')"
        class="w-full mt-1.5 flex items-center justify-center gap-2 p-2 rounded-lg text-base-content/50 hover:bg-base-200 hover:text-base-content transition-all"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-4 w-4 transition-transform duration-200"
          :class="isCollapsed ? 'rotate-180' : ''"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          stroke-width="1.75"
        >
          <path stroke-linecap="round" stroke-linejoin="round" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
        </svg>
      </button>
    </div>
  </aside>
</template>
