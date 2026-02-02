<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useUsersStore } from '@/stores/users'
import { rolesApi } from '@/api/users'
import type { User, CreateUser, UpdateUser, Role } from '@/api/types'
import AppIcon from '@/components/ui/AppIcon.vue'

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
import { getAvatarColor } from '@/utils/colors'
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
      <button class="btn btn-primary btn-sm gap-2 rounded-lg" @click="openCreateModal">
        <AppIcon name="plus" class="w-4 h-4" />
        {{ t('users.addUser') }}
      </button>
    </div>

    <!-- Alerts -->
    <div v-if="usersStore.error" class="alert alert-soft alert-error text-sm py-3 rounded-xl">
      <AppIcon name="x" class="w-5 h-5" />
      <span>{{ usersStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">
        {{ t('actions.dismiss') }}
      </button>
    </div>
    <div
      v-if="usersStore.successMessage"
      class="alert alert-soft alert-success text-sm py-3 rounded-xl"
    >
      <AppIcon name="check" class="w-5 h-5" />
      <span>{{ usersStore.successMessage }}</span>
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">
        {{ t('actions.dismiss') }}
      </button>
    </div>

    <!-- Filters & Toolbar -->
    <div class="bg-base-100 rounded-2xl border border-base-200 p-4">
      <div class="flex flex-col sm:flex-row gap-4 justify-between">
        <!-- Search -->
        <div class="relative w-full sm:w-80">
          <AppIcon
            name="search"
            class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-base-content/40"
          />
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="t('users.searchPlaceholder')"
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-xl focus:bg-base-200 focus:outline-none transition-all"
          />
        </div>

        <!-- Filters -->
        <div class="flex items-center gap-3">
          <div class="join">
            <button
              class="join-item btn btn-sm px-4"
              :class="statusFilter === 'all' ? 'btn-neutral' : 'btn-ghost bg-base-200/50'"
              @click="statusFilter = 'all'"
            >
              {{ t('users.allStatus') }}
            </button>
            <button
              class="join-item btn btn-sm px-4"
              :class="
                statusFilter === 'active' ? 'btn-success text-white' : 'btn-ghost bg-base-200/50'
              "
              @click="statusFilter = 'active'"
            >
              {{ t('users.active') }}
            </button>
            <button
              class="join-item btn btn-sm px-4"
              :class="
                statusFilter === 'inactive' ? 'btn-error text-white' : 'btn-ghost bg-base-200/50'
              "
              @click="statusFilter = 'inactive'"
            >
              {{ t('users.inactive') }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Users Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200 bg-base-50/50">
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.user') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.email') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.roles') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.quota') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.status') }}
              </th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('users.table.created') }}
              </th>
              <th class="text-xs font-medium text-base-content/50 w-20 text-right pr-6">
                {{ t('users.table.actions') }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="usersStore.isLoading">
              <td colspan="7" class="text-center py-16">
                <span class="loading loading-spinner loading-lg text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!filteredUsers.length">
              <td colspan="7" class="text-center py-16 text-base-content/50">
                <div class="flex flex-col items-center gap-2">
                  <AppIcon name="users" class="w-12 h-12 opacity-20" />
                  <p>{{ t('common.noData') }}</p>
                </div>
              </td>
            </tr>
            <tr v-for="user in filteredUsers" :key="user.id" class="group">
              <td class="font-medium">
                <div class="flex items-center gap-3">
                  <div class="avatar placeholder">
                    <div
                      class="w-10 h-10 rounded-xl text-white transition-transform hover:scale-105"
                      :class="getAvatarColor(user.username || user.firstName || 'User')"
                    >
                      <span class="text-sm font-bold">
                        {{ user.firstName[0] }}{{ user.lastName[0] }}
                      </span>
                    </div>
                  </div>
                  <div>
                    <div class="font-bold">{{ user.firstName }} {{ user.lastName }}</div>
                    <div class="text-xs text-base-content/50">@{{ user.username }}</div>
                  </div>
                </div>
              </td>
              <td class="text-sm">{{ user.email }}</td>
              <td>
                <div class="flex flex-wrap gap-1">
                  <span
                    v-for="role in user.roles"
                    :key="role"
                    class="badge badge-sm border-0 bg-primary/10 text-primary font-medium"
                  >
                    {{ role }}
                  </span>
                  <span
                    v-if="!user.roles || user.roles.length === 0"
                    class="text-xs text-base-content/40 italic"
                    >No roles</span
                  >
                </div>
              </td>
              <td>
                <div v-if="user.quota" class="w-32 group/quota">
                  <div class="flex justify-between text-xs mb-1">
                    <span class="font-medium text-base-content/70"
                      >{{ user.quota.usagePercentage }}%</span
                    >
                    <span class="text-base-content/40 group-hover/quota:text-base-content/60"
                      >{{ (user.quota.usedTokens / 1000).toFixed(1) }}k /
                      {{ (user.quota.monthlyTokenLimit / 1000).toFixed(0) }}k</span
                    >
                  </div>
                  <progress
                    class="progress w-full h-1.5 transition-all"
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
                <span v-else class="text-xs text-base-content/40">Includes Unlimited</span>
              </td>
              <td>
                <span
                  :class="[
                    'badge badge-sm border-0 font-medium',
                    user.isActive ? 'bg-success/10 text-success' : 'bg-error/10 text-error',
                  ]"
                >
                  {{ user.isActive ? t('users.active') : t('users.inactive') }}
                </span>
              </td>
              <td class="text-sm text-base-content/60">{{ formatDate(user.createdAt) }}</td>
              <td>
                <div class="flex items-center gap-1">
                  <button
                    class="btn btn-ghost btn-xs btn-square text-base-content/70 hover:text-primary hover:bg-primary/10 rounded-lg"
                    @click="openEditModal(user)"
                    :title="t('actions.edit')"
                  >
                    <AppIcon name="edit" class="w-4 h-4" />
                  </button>
                  <button
                    class="btn btn-ghost btn-xs btn-square text-base-content/70 hover:text-warning hover:bg-warning/10 rounded-lg"
                    @click="openRoleModal(user)"
                    title="Manage Roles"
                  >
                    <AppIcon name="lock" class="w-4 h-4" />
                  </button>
                  <button
                    class="btn btn-ghost btn-xs btn-square text-base-content/70 hover:text-error hover:bg-error/10 rounded-lg"
                    @click="openDeleteModal(user)"
                    :title="t('actions.delete')"
                  >
                    <AppIcon name="trash" class="w-4 h-4" />
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
      <div class="modal-box max-w-lg rounded-2xl p-6">
        <h3 class="font-bold text-xl mb-6">{{ t('users.modal.createTitle') }}</h3>
        <form @submit.prevent="handleCreate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.firstName') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="formData.firstName"
                type="text"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.lastName') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="formData.lastName"
                type="text"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.username') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="formData.username"
                type="text"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.email') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="formData.email"
                type="email"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.phone') }}
              </label>
              <input
                v-model="formData.phoneNumber"
                type="tel"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.dob') }}
              </label>
              <input
                v-model="formData.dateOfBirth"
                type="date"
                class="input input-bordered rounded-lg focus:outline-none focus:border-primary"
              />
            </div>
          </div>
          <div class="modal-action mt-8">
            <button type="button" class="btn btn-ghost rounded-lg" @click="showCreateModal = false">
              {{ t('actions.cancel') }}
            </button>
            <button
              type="submit"
              class="btn btn-primary rounded-lg px-6"
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
      <div class="modal-box max-w-lg rounded-2xl p-6">
        <h3 class="font-bold text-xl mb-6">{{ t('users.modal.editTitle') }}</h3>
        <form @submit.prevent="handleUpdate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.firstName') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="editFormData.firstName"
                type="text"
                class="input input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.lastName') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="editFormData.lastName"
                type="text"
                class="input input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.username') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="editFormData.username"
                type="text"
                class="input input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.email') }} <span class="text-error ml-1">*</span>
              </label>
              <input
                v-model="editFormData.email"
                type="email"
                class="input input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.phone') }}
              </label>
              <input
                v-model="editFormData.phoneNumber"
                type="tel"
                class="input input-bordered rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="label text-xs font-semibold uppercase text-base-content/60">
                {{ t('users.modal.dob') }}
              </label>
              <input
                v-model="editFormData.dateOfBirth"
                type="date"
                class="input input-bordered rounded-lg"
              />
            </div>
          </div>
          <div class="form-control mt-2">
            <label class="cursor-pointer label justify-start gap-4">
              <span class="label-text font-medium">{{ t('users.active') }}</span>
              <input
                v-model="editFormData.isActive"
                type="checkbox"
                class="toggle toggle-primary"
              />
            </label>
          </div>
          <div class="modal-action mt-8">
            <button type="button" class="btn btn-ghost rounded-lg" @click="showEditModal = false">
              {{ t('actions.cancel') }}
            </button>
            <button
              type="submit"
              class="btn btn-primary rounded-lg px-6"
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
      <div class="modal-box max-w-sm rounded-2xl">
        <div class="text-center">
          <div
            class="w-16 h-16 bg-error/10 text-error rounded-full flex items-center justify-center mx-auto mb-4"
          >
            <AppIcon name="trash" class="w-8 h-8" />
          </div>
          <h3 class="font-bold text-lg">{{ t('users.modal.deleteTitle') }}</h3>
          <p class="py-4 text-sm text-base-content/70">
            <span
              v-html="t('users.modal.deleteMessage', { username: userToDelete?.username })"
            ></span>
          </p>
        </div>
        <div class="flex justify-center gap-3 mt-4">
          <button class="btn btn-ghost rounded-lg" @click="showDeleteModal = false">
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-error rounded-lg"
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
      <div class="modal-box max-w-md rounded-2xl p-6">
        <!-- Header -->
        <div class="flex items-center gap-4 mb-6">
          <div class="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
            <AppIcon name="lock" class="w-6 h-6 text-primary" />
          </div>
          <div>
            <h3 class="font-bold text-lg">{{ t('users.modal.assignRole') }}</h3>
            <p class="text-sm text-base-content/60">
              For user:
              <span class="font-bold text-base-content">{{ selectedUserForRole?.username }}</span>
            </p>
          </div>
        </div>

        <!-- Current Roles -->
        <div
          v-if="selectedUserForRole?.roles?.length"
          class="mb-6 p-4 bg-base-200/50 rounded-xl border border-base-200"
        >
          <p class="text-xs font-semibold text-base-content/60 uppercase tracking-wider mb-3">
            {{ t('users.modal.currentRoles') }}
          </p>
          <div class="flex flex-wrap gap-2">
            <span
              v-for="role in selectedUserForRole.roles"
              :key="role"
              class="badge badge-lg bg-white border border-base-200"
            >
              {{ role }}
            </span>
          </div>
        </div>

        <!-- Role Selection Cards -->
        <div class="space-y-3 mb-8">
          <p class="text-xs font-semibold text-base-content/60 uppercase tracking-wider">
            {{ t('users.modal.selectRole') }}
          </p>
          <div
            v-if="!availableRoles.length"
            class="text-center py-8 text-base-content/50 bg-base-100 rounded-xl border border-dashed border-base-300"
          >
            <span class="loading loading-spinner loading-sm mb-2"></span>
            <p class="text-sm">{{ t('users.modal.loadingRoles') }}</p>
          </div>
          <div v-else class="space-y-2 max-h-60 overflow-y-auto pr-1">
            <label
              v-for="role in availableRoles"
              :key="role.id"
              :class="[
                'flex items-center gap-4 p-4 rounded-xl border-2 cursor-pointer transition-all duration-200',
                roleToAssign === role.name
                  ? 'border-primary bg-primary/5 ring-1 ring-primary/20'
                  : 'border-base-200 hover:border-primary/50 hover:bg-base-100',
              ]"
            >
              <input
                type="radio"
                name="role-select"
                :value="role.name"
                v-model="roleToAssign"
                class="radio radio-primary"
              />
              <div class="flex-1">
                <p class="font-bold text-sm">{{ role.name }}</p>
                <p class="text-xs text-base-content/60 mt-0.5 line-clamp-1">
                  {{ role.description || t('users.modal.noDescription') }}
                </p>
              </div>
            </label>
          </div>
        </div>

        <!-- Actions -->
        <div class="modal-action">
          <button class="btn btn-ghost rounded-lg" @click="showRoleModal = false">
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-primary rounded-lg px-6"
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
