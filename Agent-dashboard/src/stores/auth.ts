import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/api/auth.service'
import type { User } from 'oidc-client-ts'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => user.value !== null && !user.value.expired)
  const accessToken = computed(() => user.value?.access_token ?? null)
  const userProfile = computed(() => user.value?.profile ?? null)
  const userName = computed(
    () => userProfile.value?.preferred_username ?? userProfile.value?.name ?? 'User',
  )
  const userRoles = computed(() => {
    const realmAccess = (userProfile.value as any)?.realm_access
    return realmAccess?.roles ?? []
  })
  const isAdmin = computed(() => userRoles.value.includes('admin'))
  const userEmail = computed(() => userProfile.value?.email ?? null)

  // Actions
  async function initialize() {
    isLoading.value = true
    error.value = null
    try {
      user.value = await authService.getUser()
    } catch (e: any) {
      error.value = e.message
    } finally {
      isLoading.value = false
    }
  }

  async function login() {
    isLoading.value = true
    error.value = null
    try {
      await authService.login()
    } catch (e: any) {
      error.value = e.message
      isLoading.value = false
    }
  }

  async function handleCallback() {
    isLoading.value = true
    error.value = null
    try {
      user.value = await authService.handleCallback()
    } catch (e: any) {
      error.value = e.message
    } finally {
      isLoading.value = false
    }
  }

  async function logout() {
    isLoading.value = true
    error.value = null
    try {
      await authService.logout()
      user.value = null
    } catch (e: any) {
      error.value = e.message
    } finally {
      isLoading.value = false
    }
  }

  async function refreshToken() {
    try {
      user.value = await authService.renewToken()
    } catch (e: any) {
      error.value = e.message
    }
  }

  return {
    // State
    user,
    isLoading,
    error,
    // Getters
    isAuthenticated,
    accessToken,
    userProfile,
    userName,
    userRoles,
    isAdmin,
    userEmail,
    // Actions
    initialize,
    login,
    handleCallback,
    logout,
    refreshToken,
  }
})
