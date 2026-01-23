<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useUsersStore } from '@/stores/users'
import type { User, CreateUser, UpdateUser } from '@/api'

const usersStore = useUsersStore()

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const userToDelete = ref<User | null>(null)

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
  await usersStore.fetchUsers()
})

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

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString()
}
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">User Management</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          Manage users and their access to the system.
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
        Add User
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
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">Dismiss</button>
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
      <button class="btn btn-ghost btn-xs" @click="usersStore.clearMessages()">Dismiss</button>
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
            placeholder="Search users..."
            class="input input-sm w-full pl-9 bg-base-200/50 border-0 rounded-lg"
          />
        </div>
        <select
          v-model="statusFilter"
          class="select select-sm w-full sm:w-40 bg-base-200/50 border-0 rounded-lg"
        >
          <option value="all">All Status</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>
      </div>
    </div>

    <!-- Users Table -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="table">
          <thead>
            <tr class="border-base-200">
              <th class="text-xs font-medium text-base-content/50">User</th>
              <th class="text-xs font-medium text-base-content/50">Email</th>
              <th class="text-xs font-medium text-base-content/50">Status</th>
              <th class="text-xs font-medium text-base-content/50">Created</th>
              <th class="text-xs font-medium text-base-content/50 w-20">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="usersStore.isLoading">
              <td colspan="5" class="text-center py-12">
                <span class="loading loading-spinner loading-md text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!filteredUsers.length">
              <td colspan="5" class="text-center py-12 text-base-content/50">No users found</td>
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
                <span :class="['badge badge-sm', user.isActive ? 'badge-success' : 'badge-error']">
                  {{ user.isActive ? 'Active' : 'Inactive' }}
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
        <h3 class="font-semibold text-lg mb-4">Create New User</h3>
        <form @submit.prevent="handleCreate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">First Name *</label>
              <input
                v-model="formData.firstName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Last Name *</label>
              <input
                v-model="formData.lastName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Username *</label>
              <input
                v-model="formData.username"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Email *</label>
              <input
                v-model="formData.email"
                type="email"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Phone</label>
              <input
                v-model="formData.phoneNumber"
                type="tel"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Date of Birth</label>
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
              Cancel
            </button>
            <button
              type="submit"
              class="btn btn-primary btn-sm rounded-lg"
              :disabled="usersStore.isLoading"
            >
              <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
              Create
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
        <h3 class="font-semibold text-lg mb-4">Edit User</h3>
        <form @submit.prevent="handleUpdate" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">First Name *</label>
              <input
                v-model="editFormData.firstName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Last Name *</label>
              <input
                v-model="editFormData.lastName"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Username *</label>
              <input
                v-model="editFormData.username"
                type="text"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Email *</label>
              <input
                v-model="editFormData.email"
                type="email"
                class="input input-sm input-bordered rounded-lg"
                required
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Phone</label>
              <input
                v-model="editFormData.phoneNumber"
                type="tel"
                class="input input-sm input-bordered rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="text-xs font-medium text-base-content/60 mb-1">Date of Birth</label>
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
            <span class="text-sm">Active</span>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button
              type="button"
              class="btn btn-ghost btn-sm rounded-lg"
              @click="showEditModal = false"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="btn btn-primary btn-sm rounded-lg"
              :disabled="usersStore.isLoading"
            >
              <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
              Save
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
        <h3 class="font-semibold text-lg">Delete User</h3>
        <p class="py-4 text-sm text-base-content/70">
          Are you sure you want to delete <strong>{{ userToDelete?.username }}</strong
          >? This action cannot be undone.
        </p>
        <div class="flex justify-end gap-2">
          <button class="btn btn-ghost btn-sm rounded-lg" @click="showDeleteModal = false">
            Cancel
          </button>
          <button
            class="btn btn-error btn-sm rounded-lg"
            @click="handleDelete"
            :disabled="usersStore.isLoading"
          >
            <span v-if="usersStore.isLoading" class="loading loading-spinner loading-xs"></span>
            Delete
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="showDeleteModal = false">close</button>
      </form>
    </dialog>
  </div>
</template>
