import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/api/auth.service'
import type { User as OidcUser } from 'oidc-client-ts'
import { usersApi } from '@/api/users'
import type { User, UserPermissions } from '@/api/types'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<OidcUser | null>(null)
  const dbUser = ref<User | null>(null)
  const permissions = ref<UserPermissions | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => user.value !== null && !user.value.expired)
  const accessToken = computed(() => user.value?.access_token ?? null)
  const userProfile = computed(() => user.value?.profile ?? null)
  const userName = computed(
    () => userProfile.value?.preferred_username ?? userProfile.value?.name ?? 'User',
  )
  const userRoles = computed(() => permissions.value?.roles ?? [])
  const isAdmin = computed(() => permissions.value?.isAdmin ?? false)
  const userEmail = computed(() => userProfile.value?.email ?? null)

  // Helpers
  async function fetchDbUser(email: string) {
    try {
      const { data: users } = await usersApi.getAll()
      const existingUser = users.find((u) => u.email === email)

      if (existingUser) {
        dbUser.value = existingUser
        await fetchPermissions(existingUser.id)
      }
      return existingUser
    } catch (err) {
      console.error('Failed to fetch DB user', err)
      return null
    }
  }

  async function fetchPermissions(userId: string) {
    try {
      const response = await usersApi.getPermissions(userId)
      permissions.value = response.data
    } catch (err) {
      console.error('Failed to fetch permissions', err)
      permissions.value = null
    }
  }

  // Actions
  async function initialize() {
    isLoading.value = true
    error.value = null
    try {
      user.value = await authService.getUser()
      if (user.value?.profile?.email) {
        await fetchDbUser(user.value.profile.email)
      }
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
      const oidcUser = await authService.handleCallback()
      user.value = oidcUser

      if (oidcUser && oidcUser.profile.sub) {
        try {
          const { data: users } = await usersApi.getAll()
          const existingUser = users.find((u) => u.email === oidcUser.profile.email)

          if (existingUser) {
            dbUser.value = existingUser
            await fetchPermissions(existingUser.id)
          } else {
            const profile = oidcUser.profile
            const newUserResponse = await usersApi.create({
              username: (profile.preferred_username || profile.email || 'user') as string,
              email: (profile.email || '') as string,
              firstName: (profile.given_name ||
                (profile.name ? profile.name.split(' ')[0] : 'User')) as string,
              lastName: (profile.family_name ||
                (profile.name ? profile.name.split(' ').slice(1).join(' ') : '')) as string,
            })
            dbUser.value = newUserResponse.data
            // Fetch permissions for new user (likely basic user)
            await fetchPermissions(newUserResponse.data.id)
          }
        } catch (apiErr) {
          console.error('Error loading users API:', apiErr)
        }
      }
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
      dbUser.value = null
      permissions.value = null
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
    dbUser,
    permissions,
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
