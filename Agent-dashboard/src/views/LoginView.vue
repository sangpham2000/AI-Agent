<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

onMounted(async () => {
  await authStore.initialize()
  if (authStore.isAuthenticated) {
    router.push('/')
  } else if (!authStore.error) {
    // Auto-login only if there's no error (e.g., from a failed callback)
    authStore.login()
  }
})

function handleLogin() {
  authStore.login()
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-base-100">
    <div class="flex flex-col items-center gap-4">
      <span class="loading loading-spinner loading-lg text-primary"></span>
      <p class="text-base-content/60 animate-pulse">{{ t('login.redirecting') }}</p>
    </div>
  </div>
</template>
