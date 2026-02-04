<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { ref, onMounted } from 'vue'
import { healthApi } from '@/api'
import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import AppIcon from '@/components/ui/AppIcon.vue'

defineProps<{
  isCollapsed: boolean
}>()

const emit = defineEmits<{
  toggleMobile: []
}>()

const authStore = useAuthStore()
const apiStatus = ref<'checking' | 'online' | 'offline'>('checking')
const currentTheme = ref<'light' | 'dark'>('light')
const searchQuery = ref('')
const showNotifications = ref(false)

onMounted(async () => {
  try {
    await healthApi.check()
    apiStatus.value = 'online'
  } catch {
    apiStatus.value = 'offline'
  }

  const savedTheme = localStorage.getItem('theme')
  currentTheme.value = (savedTheme as 'light' | 'dark') || 'light'
})

function toggleTheme() {
  const html = document.documentElement
  const newTheme = currentTheme.value === 'dark' ? 'light' : 'dark'
  html.setAttribute('data-theme', newTheme)
  localStorage.setItem('theme', newTheme)
  currentTheme.value = newTheme
}
</script>

<template>
  <header
    :class="[
      'sticky top-0 z-30 h-16 flex items-center justify-between gap-4 px-4 lg:px-6 transition-all duration-200 bg-base-100 border-b border-base-200',
    ]"
  >
    <!-- Left Section: Mobile Menu & Search -->
    <div class="flex items-center gap-4 flex-1 max-w-xl">
      <!-- Mobile menu button -->
      <button class="btn btn-ghost btn-sm btn-square lg:hidden" @click="emit('toggleMobile')">
        <AppIcon name="menu-alt-2" class="w-5 h-5" />
      </button>

      <!-- Search -->
      <div class="w-full max-w-sm relative group">
        <AppIcon
          name="search"
          class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-base-content/40 group-focus-within:text-primary transition-colors"
        />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search..."
          class="input input-sm w-full pl-9 pr-12 bg-base-100/50 border-transparent hover:border-base-300 focus:bg-base-100 focus:border-primary focus:outline-none rounded-xl text-sm transition-all shadow-sm"
        />
        <div
          class="absolute right-2 top-1/2 -translate-y-1/2 flex gap-1 pointer-events-none opacity-50"
        >
          <kbd class="kbd kbd-sm h-5 min-h-0 text-[10px] bg-base-200 border-base-300">⌘</kbd>
          <kbd class="kbd kbd-sm h-5 min-h-0 text-[10px] bg-base-200 border-base-300">K</kbd>
        </div>
      </div>
    </div>

    <!-- Right Actions -->
    <div class="flex items-center gap-2">
      <!-- API Status -->
      <div
        class="tooltip tooltip-bottom"
        :data-tip="apiStatus === 'online' ? 'System Online' : 'System Offline'"
      >
        <div class="btn btn-ghost btn-xs btn-circle opacity-80">
          <span class="relative flex h-2.5 w-2.5">
            <span
              v-if="apiStatus === 'online'"
              class="animate-ping absolute inline-flex h-full w-full rounded-full bg-success opacity-75"
            ></span>
            <span
              class="relative inline-flex rounded-full h-2.5 w-2.5"
              :class="apiStatus === 'online' ? 'bg-success' : 'bg-error'"
            ></span>
          </span>
        </div>
      </div>

      <div class="h-4 w-px bg-base-content/10 mx-1"></div>

      <!-- Language -->
      <LanguageSwitcher />

      <!-- Theme Toggle -->
      <button class="btn btn-ghost btn-sm btn-square rounded-lg" @click="toggleTheme">
        <AppIcon v-if="currentTheme === 'dark'" name="sun" class="w-[18px] h-[18px]" />
        <AppIcon v-else name="moon" class="w-[18px] h-[18px]" />
      </button>

      <!-- Notifications -->
      <div class="dropdown dropdown-end">
        <button tabindex="0" class="btn btn-ghost btn-sm btn-square rounded-lg indicator">
          <AppIcon name="bell" class="w-[18px] h-[18px]" />
          <span
            class="indicator-item badge badge-xs badge-primary w-2 h-2 p-0 border-2 border-base-100 translate-y-0.5 -translate-x-0.5"
          ></span>
        </button>
        <div
          tabindex="0"
          class="dropdown-content mt-3 z-[1] w-80 bg-base-100 rounded-2xl shadow-xl border border-base-200 overflow-hidden"
        >
          <div class="p-4 border-b border-base-200 flex items-center justify-between bg-base-50/50">
            <span class="font-semibold text-sm">Notifications</span>
            <span class="badge badge-xs badge-primary badge-outline px-2 py-2">2 New</span>
          </div>
          <div class="max-h-[300px] overflow-y-auto">
            <div
              class="p-4 hover:bg-base-200/50 cursor-pointer border-b border-base-100 transition-colors flex gap-3"
            >
              <div
                class="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0 text-primary"
              >
                <AppIcon name="document-text" class="w-4 h-4" />
              </div>
              <div>
                <p class="text-sm font-medium leading-none mb-1">New document processed</p>
                <p class="text-xs text-base-content/60 leading-tight">
                  Your file "annual_report.pdf" has been successfully analyzed.
                </p>
                <p class="text-[10px] text-base-content/40 mt-1.5">2 mins ago</p>
              </div>
            </div>
            <div class="p-4 hover:bg-base-200/50 cursor-pointer transition-colors flex gap-3">
              <div
                class="w-8 h-8 rounded-full bg-success/10 flex items-center justify-center flex-shrink-0 text-success"
              >
                <AppIcon name="check-circle" class="w-4 h-4" />
              </div>
              <div>
                <p class="text-sm font-medium leading-none mb-1">System Update</p>
                <p class="text-xs text-base-content/60 leading-tight">
                  AI models have been updated to the latest version.
                </p>
                <p class="text-[10px] text-base-content/40 mt-1.5">1 hour ago</p>
              </div>
            </div>
          </div>
          <div class="p-2 border-t border-base-200 bg-base-50/50 text-center">
            <button class="btn btn-ghost btn-xs w-full text-primary">Mark all as read</button>
          </div>
        </div>
      </div>

      <!-- User Profile -->
      <div class="dropdown dropdown-end ml-1">
        <button
          tabindex="0"
          class="group flex items-center gap-2 p-1 pr-1 sm:pr-3 rounded-full border border-base-200 hover:border-base-300 hover:bg-base-100 transition-all duration-200"
        >
          <div class="avatar">
            <div
              class="w-8 h-8 rounded-full bg-gradient-to-tr from-primary to-secondary p-[2px] shadow-sm group-hover:shadow-md transition-shadow"
            >
              <div class="w-full h-full rounded-full bg-base-100 flex items-center justify-center">
                <span class="text-xs font-bold text-primary">
                  {{ authStore.userName?.[0]?.toUpperCase() || 'A' }}
                </span>
              </div>
            </div>
          </div>
          <div class="hidden sm:flex flex-col items-start pr-1">
            <span class="text-xs font-bold text-base-content leading-tight max-w-[100px] truncate">
              {{ authStore.userName || 'Admin' }}
            </span>
            <span class="text-[10px] text-base-content/50 leading-tight">Pro Plan</span>
          </div>
          <AppIcon
            name="chevron-down"
            class="w-3 h-3 text-base-content/40 group-hover:text-base-content/70 hidden sm:block transition-colors"
          />
        </button>
        <ul
          tabindex="0"
          class="dropdown-content mt-2 p-1.5 w-52 bg-base-100 rounded-2xl shadow-xl border border-base-200"
        >
          <div class="px-3 py-2 border-b border-base-100 mb-1">
            <p class="text-xs font-medium text-base-content/50">Signed in as</p>
            <p class="text-sm font-semibold truncate">
              {{ authStore.userEmail || authStore.userName || 'user@example.com' }}
            </p>
          </div>
          <li>
            <router-link
              to="/dashboard/profile"
              class="flex items-center gap-2.5 px-3 py-2 rounded-xl text-sm hover:bg-base-200/60 font-medium"
            >
              <AppIcon name="user" class="w-4 h-4 opacity-70" />
              Profile
            </router-link>
          </li>
          <li class="my-1"><div class="h-px bg-base-200 mx-2"></div></li>
          <li>
            <a
              @click="authStore.logout()"
              class="flex items-center gap-2.5 px-3 py-2 rounded-xl text-sm text-error hover:bg-error/10 font-medium"
            >
              <AppIcon name="logout" class="w-4 h-4" />
              Sign Out
            </a>
          </li>
        </ul>
      </div>
    </div>
  </header>
</template>
