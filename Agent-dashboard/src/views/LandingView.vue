<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { onMounted } from 'vue'

const authStore = useAuthStore()
const router = useRouter()

// If user is already authenticated, redirect to dashboard
onMounted(() => {
  if (authStore.isAuthenticated) {
    if (authStore.isAdmin) {
      router.push({ name: 'dashboard' })
    } else {
      // Non-admin can go to chat
      router.push({ name: 'chat' })
    }
  }
})

const handleLogin = () => {
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="hero min-h-[calc(100vh-140px)] bg-base-100">
    <div class="hero-content text-center">
      <div class="max-w-md">
        <h1 class="text-5xl font-bold">Hello there</h1>
        <p class="py-6">
          Welcome to your personal AI Agent dashboard. Manage your workflows, chat with agents, and
          explore the future of automation.
        </p>
        <button class="btn btn-primary" @click="handleLogin">Get Started</button>
      </div>
    </div>
  </div>
</template>
