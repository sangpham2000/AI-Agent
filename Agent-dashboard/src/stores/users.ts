import { defineStore } from 'pinia'
import { ref } from 'vue'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import { usersApi } from '@/api'
import type { User, CreateUser, UpdateUser } from '@/api'

dayjs.extend(utc)

export const useUsersStore = defineStore('users', () => {
  // State
  const users = ref<User[]>([])
  const selectedUser = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const successMessage = ref<string | null>(null)

  // Actions
  async function fetchUsers() {
    isLoading.value = true
    error.value = null
    try {
      const response = await usersApi.getAll()
      users.value = response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch users'
    } finally {
      isLoading.value = false
    }
  }

  async function fetchUserById(id: string) {
    isLoading.value = true
    error.value = null
    try {
      const response = await usersApi.getById(id)
      selectedUser.value = response.data
      return response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to fetch user'
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function createUser(data: CreateUser) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      const response = await usersApi.create(data)
      users.value.push(response.data)
      successMessage.value = 'User created successfully'
      return response.data
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to create user'
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function updateUser(id: string, data: UpdateUser) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      // Clean empty strings to undefined to prevent API validation errors
      const cleanedData = Object.fromEntries(
        Object.entries(data).map(([key, value]) => [key, value === '' ? undefined : value]),
      ) as UpdateUser

      // Convert dateOfBirth to UTC ISO string for PostgreSQL compatibility
      if (cleanedData.dateOfBirth) {
        cleanedData.dateOfBirth = dayjs(cleanedData.dateOfBirth).utc().toISOString()
      }

      await usersApi.update(id, cleanedData)
      const index = users.value.findIndex((u) => u.id === id)
      if (index !== -1 && users.value[index]) {
        const existingUser = users.value[index]!
        users.value[index] = {
          ...existingUser,
          ...data,
          id: existingUser.id,
          createdAt: existingUser.createdAt,
        }
      }
      successMessage.value = 'User updated successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to update user'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function deleteUser(id: string) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      await usersApi.delete(id)
      users.value = users.value.filter((u) => u.id !== id)
      successMessage.value = 'User deleted successfully'
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to delete user'
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function assignRole(id: string, roleName: string) {
    isLoading.value = true
    error.value = null
    successMessage.value = null
    try {
      await usersApi.assignRole(id, roleName)
      // Update local user state if needed
      const userIndex = users.value.findIndex((u) => u.id === id)
      if (userIndex !== -1 && users.value[userIndex]) {
        const user = users.value[userIndex]!
        if (!user.roles) user.roles = []
        if (!user.roles.includes(roleName)) {
          user.roles.push(roleName)
        }
      }
      if (selectedUser.value && selectedUser.value.id === id) {
        if (!selectedUser.value.roles) selectedUser.value.roles = []
        if (!selectedUser.value.roles.includes(roleName)) {
          selectedUser.value.roles.push(roleName)
        }
      }

      successMessage.value = `Role '${roleName}' assigned successfully`
      return true
    } catch (e: any) {
      error.value = e.response?.data?.message || e.message || 'Failed to assign role'
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearMessages() {
    error.value = null
    successMessage.value = null
  }

  function setSelectedUser(user: User | null) {
    selectedUser.value = user
  }

  return {
    users,
    selectedUser,
    isLoading,
    error,
    successMessage,
    fetchUsers,
    fetchUserById,
    createUser,
    updateUser,
    deleteUser,
    assignRole,
    clearMessages,
    setSelectedUser,
  }
})
