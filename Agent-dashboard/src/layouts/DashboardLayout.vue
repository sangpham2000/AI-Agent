<script setup lang="ts">
import { ref, onMounted, provide } from 'vue'
import Sidebar from '@/components/layout/Sidebar.vue'
import Header from '@/components/layout/Header.vue'
import { RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

// Sidebar state
const isCollapsed = ref(false)
const isMobileOpen = ref(false)

provide('sidebarCollapsed', isCollapsed)

const toggleCollapse = () => {
  isCollapsed.value = !isCollapsed.value
  localStorage.setItem('sidebar-collapsed', String(isCollapsed.value))
}

const toggleMobile = () => {
  isMobileOpen.value = !isMobileOpen.value
}

const closeMobile = () => {
  isMobileOpen.value = false
}

onMounted(async () => {
  await authStore.initialize()

  const savedCollapsed = localStorage.getItem('sidebar-collapsed')
  if (savedCollapsed === 'true') {
    isCollapsed.value = true
  }

  const savedTheme = localStorage.getItem('theme')
  if (savedTheme) {
    document.documentElement.setAttribute('data-theme', savedTheme)
  } else {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    document.documentElement.setAttribute('data-theme', prefersDark ? 'dark' : 'light')
  }
})
</script>

<template>
  <div class="min-h-screen bg-base-200">
    <!-- Sidebar -->
    <Sidebar
      :is-collapsed="isCollapsed"
      :is-mobile-open="isMobileOpen"
      @toggle-collapse="toggleCollapse"
      @close-mobile="closeMobile"
    />

    <!-- Main Content -->
    <div
      :class="[
        'min-h-screen flex flex-col transition-all duration-200',
        isCollapsed ? 'lg:ml-16' : 'lg:ml-56',
      ]"
    >
      <!-- Header -->
      <Header :is-collapsed="isCollapsed" @toggle-mobile="toggleMobile" />

      <!-- Page Content -->
      <main class="flex-1 p-4 lg:p-5">
        <RouterView v-slot="{ Component }">
          <transition name="page" mode="out-in">
            <component :is="Component" />
          </transition>
        </RouterView>
      </main>

      <!-- Footer -->
      <footer class="py-3 px-5 text-center text-xs text-base-content/40 border-t border-base-200">
        Built and designed with care by <a href="#" class="text-primary hover:underline">Phạm Thanh Sang</a>
      </footer>
    </div>
  </div>
</template>

<style>
.page-enter-active,
.page-leave-active {
  transition: opacity 0.15s ease;
}

.page-enter-from,
.page-leave-to {
  opacity: 0;
}
</style>
