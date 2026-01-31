<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useUsersStore } from '@/stores/users'
import { rolesApi } from '@/api/users'
import type { User, CreateUser, UpdateUser, Role } from '@/api/types'

const { t } = useI18n()
const usersStore = useUsersStore()

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const showRoleModal = ref(false)
const userToDelete = ref<User | null>(null)
const selectedUserForRole = ref<User | null>(null)
const roleToAssign = ref('admin')
const availableRoles = ref<Role[]>([])

// Form state
const formData = ref<CreateUser>({
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  dateOfBirth: '',
  avatarUrl: '',
})

const editFormData = ref<UpdateUser>({
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  dateOfBirth: '',
  avatarUrl: '',
  isActive: true,
})

// Search & Filter
const searchQuery = ref('')
const statusFilter = ref<'all' | 'active' | 'inactive'>('all')

const filteredUsers = computed(() => {
  let result = usersStore.users

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(
      (u) =>
        u.username.toLowerCase().includes(query) ||
        u.email.toLowerCase().includes(query) ||
        u.firstName.toLowerCase().includes(query) ||
        u.lastName.toLowerCase().includes(query),
    )
  }

  if (statusFilter.value === 'active') {
    result = result.filter((u) => u.isActive)
  } else if (statusFilter.value === 'inactive') {
    result = result.filter((u) => !u.isActive)
  }

  return result
})

onMounted(async () => {
  await Promise.all([usersStore.fetchUsers(), fetchRoles()])
})

async function fetchRoles() {
  try {
    const response = await rolesApi.getAll()
    availableRoles.value = response.data
  } catch (error) {
    console.error('Failed to fetch roles', error)
  }
}

// Actions
function openCreateModal() {
  formData.value = {
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    dateOfBirth: '',
    avatarUrl: '',
  }
  showCreateModal.value = true
}

function openEditModal(user: User) {
  editFormData.value = {
    username: user.username,
    email: user.email,
    firstName: user.firstName,
    lastName: user.lastName,
    phoneNumber: user.phoneNumber || '',
    dateOfBirth: user.dateOfBirth || '',
    avatarUrl: user.avatarUrl || '',
    isActive: user.isActive,
  }
  usersStore.setSelectedUser(user)
  showEditModal.value = true
}

function openDeleteModal(user: User) {
  userToDelete.value = user
  showDeleteModal.value = true
}

async function handleCreate() {
  const result = await usersStore.createUser(formData.value)
  if (result) {
    showCreateModal.value = false
  }
}

async function handleUpdate() {
  if (!usersStore.selectedUser) return
  const result = await usersStore.updateUser(usersStore.selectedUser.id, editFormData.value)
  if (result) {
    showEditModal.value = false
    usersStore.setSelectedUser(null)
  }
}

async function handleDelete() {
  if (!userToDelete.value) return
  const result = await usersStore.deleteUser(userToDelete.value.id)
  if (result) {
    showDeleteModal.value = false
    userToDelete.value = null
  }
}

function openRoleModal(user: User) {
  selectedUserForRole.value = user
  roleToAssign.value = '' // Don't pre-select any role
  showRoleModal.value = true
}

async function handleAssignRole() {
  if (!selectedUserForRole.value) return
  const result = await usersStore.assignRole(selectedUserForRole.value.id, roleToAssign.value)
  if (result) {
    showRoleModal.value = false
    selectedUserForRole.value = null
  }
}

import { formatDate } from '@/utils/format'
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">{{ t('users.title') }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          {{ t('users.subtitle') }}
        </p>
      </div>
      <button class="btn btn-primary btn-sm gap-1.5 rounded-lg" @click="openCreateModal">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          class="h-4 w-4"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          stroke-width="2"
        >
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
        {{ t('users.addUser') }}
      </button>
    </div>

    <!-- Alerts -->
    <div v-if="usersStore.error" class="alert alert-error text-sm py-3">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        class="h-5 w-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
        />
      </svg>
      <span>{{ usersStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">
        {{ t('actions.dismiss') }}
      </button>
    </div>
    <div v-if="usersStore.successMessage" class="alert alert-success text-sm py-3">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        class="h-5 w-5"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
      </svg>
      <span>{{ usersStore.successMessage }}</span>
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">
        {{ t('actions.dismiss') }}
      </button>
    </div>

    <!-- Filters -->
    <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-base-content/40"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="t('users.searchPlaceholder')"
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-lg"
          />
        </div>
        <select
          v-model="statusFilter"
          class="select select-sm w-full sm:w-40 bg-base-200/50 border-0 rounded-lg"
        >
          <option value="all">{{ t('users.allStatus') }}</option>
          <option value="active">{{ t('users.active') }}</option>
          <option value="inactive">{{ t('users.inactive') }}</option>
        </select>
      </div>
    </div>

    <!-- Users Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200">
              <th class="text-xs font-medium text-base-content/50">{{ t('users.table.user') }}</th>
              <th class="text-xs font-medium text-base-content/50">{{ t('users.table.email') }}</th>
              <th class="text-xs font-medium text-base-content/50">{{ t('users.table.roles') }}</th>
              <th class="text-xs font-medium text-base-content/50">{{ t('users.table.quota') }}</th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.status') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.created') }}
              </th>
              <th class="text-xs font-medium text-base-content/50 w-20">
                {{ t('users.table.actions') }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="usersStore.isLoading">
              <td colspan="5" class="text-center py-12">
                <span class="loading loading-spinner loading-md text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!filteredUsers.length">
              <td colspan="6" class="text-center py-12 text-base-content/50">
                {{ t('common.noData') }}
              </td>
            </tr>
            <tr v-for="user in filteredUsers" :key="user.id" class="hover border-base-200">
              <td>
                <div class="flex items-center gap-3">
                  <div class="avatar">
                    <div class="w-9 h-9 rounded-lg bg-gradient-to-br from-primary to-secondary">
                      <span
                        class="flex items-center justify-center h-full text-xs font-semibold text-white"
                      >
                        {{ user.firstName[0] }}{{ user.lastName[0] }}
                      </span>
                    </div>
                  </div>
                  <div>
                    <p class="font-medium text-sm">{{ user.firstName }} {{ user.lastName }}</p>
                    <p class="text-xs text-base-content/50">@{{ user.username }}</p>
                  </div>
                </div>
              </td>
              <td class="text-sm">{{ user.email }}</td>
              <td>
                <div class="flex flex-wrap gap-1">
                  <span
                    v-for="role in user.roles"
                    :key="role"
                    class="badge badge-sm border-0 bg-secondary/10 text-secondary"
                  >
                    {{ role }}
                  </span>
                  <span
                    v-if="!user.roles || user.roles.length === 0"
                    class="text-xs text-base-content/40"
                    >-</span
                  >
                </div>
              </td>
              <td>
                <div v-if="user.quota" class="flex flex-col gap-1 w-32">
                  <div class="flex justify-between text-xs">
                    <span class="font-medium text-base-content/70"
                      >{{ user.quota.usagePercentage }}%</span
                    >
                    <span class="text-base-content/40"
                      >{{ (user.quota.usedTokens / 1000).toFixed(1) }}k /
                      {{ (user.quota.monthlyTokenLimit / 1000).toFixed(0) }}k</span
                    >
                  </div>
                  <progress
                    class="progress w-full h-1.5"
                    :class="[
                      user.quota.usagePercentage > 90
                        ? 'progress-error'
                        : user.quota.usagePercentage > 75
                          ? 'progress-warning'
                          : 'progress-primary',
                    ]"
                    :value="user.quota.usedTokens"
                    :max="user.quota.monthlyTokenLimit"
                  ></progress>
                </div>
                <span v-else class="text-xs text-base-content/40">N/A</span>
              </td>
              <td>
                <span
                  :class="[
                    'badge badge-sm border-0',
                    user.isActive ? 'bg-success/10 text-success' : 'bg-error/10 text-error',
                  ]"
                >
                  {{ user.isActive ? t('users.active') : t('users.inactive') }}
                </span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatDate(user.createdAt) }}</td>
              <td>
                <div class="flex gap-1">
                  <button
                    class="btn btn-ghost btn-xs btn-square"
                    @click="openEditModal(user)"
                    title="Edit"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-4 w-4"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      stroke-width="1.75"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                      />
                    </svg>
                  </button>
                  <button
                    class="btn btn-ghost btn-xs btn-square text-warning"
                    @click="openRoleModal(user)"
                    title="Manage Roles"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-4 w-4"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      stroke-width="1.75"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                      />
                    </svg>
                  </button>
                  <button
                    class="btn btn-ghost btn-xs btn-square text-error"
                    @click="openDeleteModal(user)"
                    title="Delete"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-4 w-4"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      stroke-width="1.75"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                      />
                    </svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Modal -->
    <dialog :class="['modal', { 'modal-open': showCreateModal }]">
      <div class="modal-box max-w-lg">
        <h3 class="font-semibold text-lg mb-4">{{ t('users.modal.createTitle') }}</h3>
        <form @submit.prevent="handleCreate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.firstName') }} *
              </label>
              <input
                v-model="formData.firstName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.lastName') }} *
              </label>
              <input
                v-model="formData.lastName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.username') }} *
              </label>
              <input
                v-model="formData.username"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.email') }} *
              </label>
              <input
                v-model="formData.email"
                type="email"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.phone') }}
              </label>
              <input
                v-model="formData.phoneNumber"
                type="tel"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.dob') }}
              </label>
              <input
                v-model="formData.dateOfBirth"
                type="date"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              class="btn btn-ghost btn-sm rounded-lg"
              @click="showCreateModal = false"
            >
              {{ t('actions.cancel') }}
            </button>
            <button
              type="submit"
              class="btn btn-primary btn-sm rounded-lg"
              :disabled="usersStore.isLoading"
            >
              <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
              {{ t('actions.create') }}
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showCreateModal = false">close</button>
      </form>
    </dialog>

    <!-- Edit Modal -->
    <dialog :class="['modal', { 'modal-open': showEditModal }]">
      <div class="modal-box max-w-lg">
        <h3 class="font-semibold text-lg mb-4">{{ t('users.modal.editTitle') }}</h3>
        <form @submit.prevent="handleUpdate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.firstName') }} *
              </label>
              <input
                v-model="editFormData.firstName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.lastName') }} *
              </label>
              <input
                v-model="editFormData.lastName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.username') }} *
              </label>
              <input
                v-model="editFormData.username"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.email') }} *
              </label>
              <input
                v-model="editFormData.email"
                type="email"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.phone') }}
              </label>
              <input
                v-model="editFormData.phoneNumber"
                type="tel"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">
                {{ t('users.modal.dob') }}
              </label>
              <input
                v-model="editFormData.dateOfBirth"
                type="date"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
          </div>
          <div class="flex items-center gap-2 pt-2">
            <input
              v-model="editFormData.isActive"
              type="checkbox"
              class="toggle toggle-sm toggle-primary"
            />
            <span class="text-sm">{{ t('users.active') }}</span>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              class="btn btn-ghost btn-sm rounded-lg"
              @click="showEditModal = false"
            >
              {{ t('actions.cancel') }}
            </button>
            <button
              type="submit"
              class="btn btn-primary btn-sm rounded-lg"
              :disabled="usersStore.isLoading"
            >
              <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
              {{ t('actions.save') }}
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showEditModal = false">close</button>
      </form>
    </dialog>

    <!-- Delete Modal -->
    <dialog :class="['modal', { 'modal-open': showDeleteModal }]">
      <div class="modal-box max-w-sm">
        <h3 class="font-semibold text-lg">{{ t('users.modal.deleteTitle') }}</h3>
        <p class="py-4 text-sm text-base-content/70">
          <span
            v-html="t('users.modal.deleteMessage', { username: userToDelete?.username })"
          ></span>
        </p>
        <div class="flex justify-end gap-2">
          <button class="btn btn-ghost btn-sm rounded-lg" @click="showDeleteModal = false">
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-error btn-sm rounded-lg"
            @click="handleDelete"
            :disabled="usersStore.isLoading"
          >
            <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
            {{ t('actions.delete') }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showDeleteModal = false">close</button>
      </form>
    </dialog>

    <!-- Role Modal -->
    <dialog :class="['modal', { 'modal-open': showRoleModal }]">
      <div class="modal-box max-w-md">
        <!-- Header -->
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-5 w-5 text-primary"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
              />
            </svg>
          </div>
          <div>
            <h3 class="font-semibold text-lg">{{ t('users.modal.assignRole') }}</h3>
            <p class="text-xs text-base-content/60">
              {{ t('users.modal.roleFor') }}
              <span class="font-medium text-base-content">{{ selectedUserForRole?.username }}</span>
            </p>
          </div>
        </div>

        <!-- Current Roles -->
        <div v-if="selectedUserForRole?.roles?.length" class="mb-4 p-3 bg-base-200/50 rounded-lg">
          <p class="text-xs text-base-content/60 mb-2">{{ t('users.modal.currentRoles') }}</p>
          <div class="flex flex-wrap gap-1.5">
            <span
              v-for="role in selectedUserForRole.roles"
              :key="role"
              class="badge badge-sm bg-secondary/10 text-secondary border-0"
            >
              {{ role }}
            </span>
          </div>
        </div>

        <!-- Role Selection Cards -->
        <div class="space-y-2 mb-5">
          <p class="text-xs font-medium text-base-content/60 uppercase tracking-wider">
            {{ t('users.modal.selectRole') }}
          </p>
          <div v-if="!availableRoles.length" class="text-center py-6 text-base-content/50">
            <span class="loading loading-spinner loading-sm"></span>
            <p class="mt-2 text-sm">{{ t('users.modal.loadingRoles') }}</p>
          </div>
          <div v-else class="space-y-2 max-h-60 overflow-y-auto pr-1">
            <label
              v-for="role in availableRoles"
              :key="role.id"
              :class="[
                'flex items-start gap-3 p-3 rounded-xl border-2 cursor-pointer transition-all duration-150',
                roleToAssign === role.name
                  ? 'border-primary bg-primary/5'
                  : 'border-base-200 hover:border-base-300 hover:bg-base-200/30',
              ]"
            >
              <input
                type="radio"
                name="role-select"
                :value="role.name"
                v-model="roleToAssign"
                class="radio radio-primary radio-sm mt-0.5"
              />
              <div class="flex-1 min-w-0">
                <p class="font-medium text-sm">{{ role.name }}</p>
                <p class="text-xs text-base-content/60 mt-0.5">
                  {{ role.description || t('users.modal.noDescription') }}
                </p>
              </div>
            </label>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex justify-end gap-2 pt-2 border-t border-base-200">
          <button class="btn btn-ghost btn-sm rounded-lg" @click="showRoleModal = false">
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-primary btn-sm rounded-lg"
            @click="handleAssignRole"
            :disabled="usersStore.isLoading || !roleToAssign"
          >
            <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
            {{ t('users.modal.assignRole') }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showRoleModal = false">close</button>
      </form>
    </dialog>
  </div>
</template>
