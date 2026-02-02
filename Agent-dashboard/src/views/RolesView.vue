<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { rolesApi } from '@/api/users'
import type { Role, Permission } from '@/api/types'

const { t } = useI18n()
const roles = ref<Role[]>([])
const allPermissions = ref<Permission[]>([])
const selectedRole = ref<Role | null>(null)
const selectedPermissionIds = ref<Set<string>>(new Set())
const isLoading = ref(false)
const isSaving = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

onMounted(async () => {
  await Promise.all([fetchRoles(), fetchPermissions()])
})

async function fetchRoles() {
  isLoading.value = true
  try {
    const res = await rolesApi.getAll()
    roles.value = res.data
    // Select first role by default if available
    if (roles.value.length > 0 && !selectedRole.value) {
      selectRole(roles.value[0]!)
    }
  } catch (error) {
    console.error('Failed to fetch roles', error)
    console.error('Failed to fetch roles', error)
    errorMessage.value = t('roles.messages.loadError')
  } finally {
    isLoading.value = false
  }
}

async function fetchPermissions() {
  try {
    const res = await rolesApi.getPermissions()
    allPermissions.value = res.data
  } catch (error) {
    console.error('Failed to fetch permissions', error)
    console.error('Failed to fetch permissions', error)
    errorMessage.value = t('roles.messages.permError')
  }
}

function selectRole(role: Role) {
  selectedRole.value = role
  // Initialize checked permissions based on the role's current permissions
  selectedPermissionIds.value = new Set(role.permissions?.map((p) => p.id) || [])
}

async function savePermissions() {
  if (!selectedRole.value) return

  isSaving.value = true
  successMessage.value = ''
  errorMessage.value = ''

  try {
    const permissionIds = Array.from(selectedPermissionIds.value)
    await rolesApi.updatePermissions(selectedRole.value.id, permissionIds)

    // Refresh roles to update local state
    await fetchRoles()
    // Re-select the role to keep UI in sync (reload permissions from updated role)
    const updatedRole = roles.value.find((r) => r.id === selectedRole.value?.id)
    if (updatedRole) {
      selectRole(updatedRole)
    }

    successMessage.value = t('roles.messages.saveSuccess')
    setTimeout(() => (successMessage.value = ''), 3000)
  } catch (error) {
    console.error('Failed to save permissions', error)
    errorMessage.value = t('roles.messages.saveError')
  } finally {
    isSaving.value = false
  }
}

// Group permissions by 'group' field
const groupedPermissions = computed(() => {
  const groups: Record<string, Permission[]> = {}
  allPermissions.value.forEach((p) => {
    const groupName = p.group || 'Other'
    if (!groups[groupName]) {
      groups[groupName] = []
    }
    groups[groupName].push(p)
  })
  return groups
})

function togglePermission(permId: string) {
  if (selectedPermissionIds.value.has(permId)) {
    selectedPermissionIds.value.delete(permId)
  } else {
    selectedPermissionIds.value.add(permId)
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">{{ t('roles.title') }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">{{ t('roles.subtitle') }}</p>
      </div>
    </div>

    <!-- Feedback Alerts -->
    <div v-if="errorMessage" class="alert alert-soft alert-error text-sm py-3 rounded-xl">
      <span>{{ errorMessage }}</span>
      <button class="btn btn-ghost btn-xs" @click="errorMessage = ''">
        {{ t('actions.dismiss') }}
      </button>
    </div>
    <div v-if="successMessage" class="alert alert-soft alert-success text-sm py-3 rounded-xl">
      <span>{{ successMessage }}</span>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6">
      <!-- Role List (Sidebar) -->
      <div
        class="lg:col-span-1 bg-base-100 rounded-2xl border border-base-200 overflow-hidden h-fit"
      >
        <div class="p-4 border-b border-base-200 bg-base-200/30">
          <h3 class="font-semibold">{{ t('roles.rolesList') }}</h3>
        </div>
        <div v-if="isLoading && roles.length === 0" class="p-4 text-center">
          <span class="loading loading-spinner text-primary"></span>
        </div>
        <ul v-else class="menu p-2 rounded-box">
          <li v-for="role in roles" :key="role.id">
            <a
              @click="selectRole(role)"
              :class="{ active: selectedRole?.id === role.id }"
              class="justify-between"
            >
              <span class="font-medium">{{ role.name }}</span>
              <span class="badge badge-sm badge-ghost">{{ role.permissions?.length || 0 }}</span>
            </a>
          </li>
        </ul>
      </div>

      <!-- Permission Editor (Main Content) -->
      <div
        v-if="selectedRole"
        class="lg:col-span-3 bg-base-100 rounded-2xl border border-base-200 overflow-hidden flex flex-col"
      >
        <div
          class="p-4 border-b border-base-200 bg-base-200/30 flex justify-between items-center sticky top-0 z-10 backdrop-blur-sm"
        >
          <div>
            <h3 class="font-semibold text-lg flex items-center gap-2">
              {{ t('roles.permissionsFor') }}
              <span class="text-primary">{{ selectedRole.name }}</span>
            </h3>
            <p class="text-xs text-base-content/60">{{ selectedRole.description }}</p>
          </div>
          <button
            class="btn btn-primary btn-sm rounded-lg shadow-sm"
            @click="savePermissions"
            :disabled="isSaving"
          >
            <span v-if="isSaving" class="loading loading-spinner loading-xs"></span>
            {{ t('roles.saveChanges') }}
          </button>
        </div>

        <div class="p-6 overflow-y-auto max-h-[70vh]">
          <div v-if="allPermissions.length === 0" class="text-center py-12 text-base-content/50">
            {{ t('roles.noPermissions') }}
          </div>

          <div
            v-for="(perms, groupName) in groupedPermissions"
            :key="groupName"
            class="mb-8 last:mb-0"
          >
            <h4
              class="font-bold text-sm uppercase tracking-wider text-base-content/40 mb-3 border-b border-base-200 pb-1"
            >
              {{ groupName }}
            </h4>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div
                v-for="perm in perms"
                :key="perm.id"
                class="form-control hover:bg-base-200/50 rounded-lg p-2 transition-colors border border-transparent hover:border-base-200 cursor-pointer"
                @click="togglePermission(perm.id)"
              >
                <label class="label cursor-pointer justify-start gap-3 p-0 pointer-events-none">
                  <input
                    type="checkbox"
                    :checked="selectedPermissionIds.has(perm.id)"
                    class="checkbox checkbox-sm checkbox-primary"
                    readonly
                  />
                  <div>
                    <span class="label-text font-medium block">{{ perm.name }}</span>
                    <span class="label-text-alt text-xs text-base-content/60">{{
                      perm.description
                    }}</span>
                  </div>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div
        v-else
        class="lg:col-span-3 flex items-center justify-center p-12 bg-base-100 rounded-2xl border border-base-200 border-dashed text-base-content/40"
      >
        {{ t('roles.selectRole') }}
      </div>
    </div>
  </div>
</template>
