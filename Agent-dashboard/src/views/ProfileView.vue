<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import usersApi from '@/api/users'
import type { UserQuota } from '@/api/types'
import AppIcon from '@/components/ui/AppIcon.vue'
import { formatDate } from '@/utils/format'
import { getAvatarColor } from '@/utils/colors'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'

dayjs.extend(utc)

const { t } = useI18n()
const authStore = useAuthStore()
const quota = ref<UserQuota | null>(null)
const isLoading = ref(false)
const isEditing = ref(false)
const isSaving = ref(false)

const editForm = ref({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  dateOfBirth: '',
})

// Tính toán avatar color dựa trên username
const avatarColorClass = computed(() => {
  return getAvatarColor(authStore.dbUser?.username || authStore.dbUser?.firstName || 'User')
})

onMounted(async () => {
  if (authStore.dbUser?.id) {
    try {
      isLoading.value = true
      const response = await usersApi.getQuota(authStore.dbUser.id)
      quota.value = response.data
    } catch (error) {
      console.error('Failed to fetch quota', error)
    } finally {
      isLoading.value = false
    }
  }
})

function getInitials(firstName: string, lastName: string) {
  const f = firstName ? firstName.charAt(0).toUpperCase() : ''
  const l = lastName ? lastName.charAt(0).toUpperCase() : ''
  return f + l || 'U'
}

function openEditModal() {
  if (authStore.dbUser) {
    editForm.value = {
      firstName: authStore.dbUser.firstName || '',
      lastName: authStore.dbUser.lastName || '',
      phoneNumber: authStore.dbUser.phoneNumber || '',
      dateOfBirth: authStore.dbUser.dateOfBirth
        ? dayjs(authStore.dbUser.dateOfBirth).format('YYYY-MM-DD')
        : '',
    }
    isEditing.value = true
  }
}

async function updateProfile() {
  if (!authStore.dbUser) return

  isSaving.value = true
  try {
    const payload = {
      ...authStore.dbUser,
      firstName: editForm.value.firstName,
      lastName: editForm.value.lastName,
      phoneNumber: editForm.value.phoneNumber || undefined,
      dateOfBirth: editForm.value.dateOfBirth
        ? dayjs(editForm.value.dateOfBirth).utc().toISOString()
        : undefined,
      lastLoginAt: authStore.dbUser.lastLoginAt
        ? dayjs(authStore.dbUser.lastLoginAt).utc().toISOString()
        : undefined,
    }

    if (!payload.phoneNumber) delete (payload as any).phoneNumber

    await usersApi.update(authStore.dbUser.id, payload)
    await authStore.initialize()
    isEditing.value = false
  } catch (error) {
    console.error('Failed to update profile', error)
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <div class="space-y-5">
    <!-- Hero Header Card -->
    <div class="bg-base-100 rounded-2xl border border-base-200 overflow-hidden">
      <!-- Gradient Banner -->
      <div class="h-24 bg-gradient-to-r from-primary/20 via-secondary/10 to-accent/20"></div>

      <!-- Profile Info -->
      <div class="px-6 pb-6">
        <div class="flex flex-col md:flex-row md:items-end gap-4 -mt-12">
          <!-- Avatar -->
          <div class="relative">
            <div
              class="w-24 h-24 rounded-2xl ring-4 ring-base-100 flex items-center justify-center overflow-hidden text-white"
              :class="avatarColorClass"
            >
              <span v-if="!authStore.user?.profile?.avatar" class="text-3xl font-bold">
                {{
                  getInitials(authStore.dbUser?.firstName || '', authStore.dbUser?.lastName || '')
                }}
              </span>
              <img
                v-else
                :src="String(authStore.user?.profile?.avatar || '')"
                alt="Avatar"
                class="w-full h-full object-cover"
              />
            </div>
            <button
              class="btn btn-circle btn-xs btn-primary absolute -bottom-1 -right-1 ring-2 ring-base-100"
            >
              <AppIcon name="edit" class="w-3 h-3" />
            </button>
          </div>

          <!-- Name & Username -->
          <div class="flex-1 md:pb-1">
            <h1 class="text-xl font-semibold">
              {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
            </h1>
            <p class="text-sm text-base-content/50">@{{ authStore.dbUser?.username }}</p>
          </div>

          <!-- Actions -->
          <div class="flex gap-2">
            <button class="btn btn-primary btn-sm gap-2 rounded-lg" @click="openEditModal">
              <AppIcon name="edit" class="w-4 h-4" />
              {{ t('profile.editProfile') }}
            </button>
          </div>
        </div>

        <!-- Role Badges -->
        <div class="flex gap-2 mt-4">
          <span
            v-for="role in authStore.userRoles"
            :key="role"
            class="badge badge-sm bg-primary/10 text-primary border-0"
          >
            {{ role }}
          </span>
          <span class="badge badge-sm bg-success/10 text-success border-0">
            <AppIcon name="check" class="w-3 h-3 mr-1" />
            {{ t('profile.verified') }}
          </span>
        </div>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chat" class="w-3.5 h-3.5" />
          {{ t('profile.stats.conversations') }}
        </p>
        <p class="text-2xl font-bold text-primary">128</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="document" class="w-3.5 h-3.5" />
          {{ t('profile.stats.documents') }}
        </p>
        <p class="text-2xl font-bold text-secondary">42</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="clock" class="w-3.5 h-3.5" />
          {{ t('profile.stats.activeTime') }}
        </p>
        <p class="text-2xl font-bold">24h</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="users" class="w-3.5 h-3.5" />
          {{ t('profile.joined') }}
        </p>
        <p class="text-lg font-bold">{{ formatDate(authStore.dbUser?.createdAt) }}</p>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Personal Information Card -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center gap-2 mb-5">
          <div class="w-8 h-8 rounded-xl bg-primary/10 flex items-center justify-center">
            <AppIcon name="user" class="w-4 h-4 text-primary" />
          </div>
          <h3 class="font-semibold">{{ t('profile.personalInfo') }}</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <div class="bg-base-200/30 rounded-xl p-4">
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">
              {{ t('profile.fullName') }}
            </label>
            <p class="font-medium mt-1">
              {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
            </p>
          </div>
          <div class="bg-base-200/30 rounded-xl p-4">
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">
              {{ t('profile.email') }}
            </label>
            <p class="font-medium mt-1 truncate">{{ authStore.dbUser?.email }}</p>
          </div>
          <div class="bg-base-200/30 rounded-xl p-4">
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">
              {{ t('profile.phone') }}
            </label>
            <p
              class="font-medium mt-1"
              :class="authStore.dbUser?.phoneNumber ? '' : 'text-base-content/40 italic'"
            >
              {{ authStore.dbUser?.phoneNumber || t('profile.notProvided') }}
            </p>
          </div>
          <div class="bg-base-200/30 rounded-xl p-4">
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">
              {{ t('profile.joined') }}
            </label>
            <p class="font-medium mt-1">{{ formatDate(authStore.dbUser?.createdAt) }}</p>
          </div>
        </div>
      </div>

      <!-- Right Column -->
      <div class="space-y-5">
        <!-- Quota Status -->
        <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
          <div class="flex items-center gap-2 mb-5">
            <div class="w-8 h-8 rounded-xl bg-secondary/10 flex items-center justify-center">
              <AppIcon name="chart" class="w-4 h-4 text-secondary" />
            </div>
            <h3 class="font-semibold">{{ t('profile.quota.title') }}</h3>
          </div>

          <div v-if="isLoading" class="flex justify-center py-8">
            <span class="loading loading-spinner text-primary"></span>
          </div>

          <template v-else-if="quota">
            <div class="flex flex-col items-center mb-5">
              <div
                class="radial-progress text-primary bg-base-200/50 text-sm font-bold"
                :style="`--value:${quota.usagePercentage}; --size:7rem; --thickness: 0.5rem;`"
                role="progressbar"
              >
                {{ quota.usagePercentage }}%
              </div>
              <p class="mt-3 text-xs font-medium text-base-content/50 uppercase">
                {{ t('profile.quota.monthlyLimit') }}
              </p>
              <p class="text-xl font-bold">{{ quota.monthlyTokenLimit.toLocaleString() }}</p>
            </div>

            <div class="space-y-2 pt-4 border-t border-base-200">
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">{{ t('profile.quota.used') }}</span>
                <span class="font-medium">{{ quota.usedTokens.toLocaleString() }}</span>
              </div>
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">{{ t('profile.quota.remaining') }}</span>
                <span class="font-medium text-success">{{
                  quota.remainingTokens.toLocaleString()
                }}</span>
              </div>
            </div>
          </template>
          <div v-else class="text-center py-8 text-base-content/40 text-sm">
            {{ t('profile.quota.noData') }}
          </div>
        </div>

        <!-- Account Plan -->
        <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
          <div class="flex items-center justify-between mb-3">
            <h3 class="font-semibold">{{ t('profile.plan.title') }}</h3>
            <span class="badge badge-primary badge-sm">{{ t('profile.plan.pro') }}</span>
          </div>
          <p class="text-sm text-base-content/60 mb-4">
            {{ t('profile.plan.description') }}
          </p>
          <button class="btn btn-sm btn-outline w-full rounded-lg">
            {{ t('profile.plan.manage') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Edit Profile Modal -->
    <dialog class="modal" :class="{ 'modal-open': isEditing }">
      <div class="modal-box max-w-md">
        <button
          class="btn btn-sm btn-circle btn-ghost absolute right-4 top-4"
          @click="isEditing = false"
        >
          ✕
        </button>
        <h3 class="font-bold text-lg mb-5">{{ t('profile.modal.title') }}</h3>
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="label pb-1">
                <span class="label-text text-xs font-medium">{{
                  t('profile.modal.firstName')
                }}</span>
              </label>
              <input
                v-model="editForm.firstName"
                type="text"
                class="input input-bordered input-sm w-full rounded-lg"
              />
            </div>
            <div class="form-control">
              <label class="label pb-1">
                <span class="label-text text-xs font-medium">{{
                  t('profile.modal.lastName')
                }}</span>
              </label>
              <input
                v-model="editForm.lastName"
                type="text"
                class="input input-bordered input-sm w-full rounded-lg"
              />
            </div>
          </div>
          <div class="form-control">
            <label class="label pb-1">
              <span class="label-text text-xs font-medium">{{ t('profile.modal.phone') }}</span>
            </label>
            <input
              v-model="editForm.phoneNumber"
              type="tel"
              class="input input-bordered input-sm w-full rounded-lg"
            />
          </div>
          <div class="form-control">
            <label class="label pb-1">
              <span class="label-text text-xs font-medium">{{ t('profile.modal.dob') }}</span>
            </label>
            <input
              v-model="editForm.dateOfBirth"
              type="date"
              class="input input-bordered input-sm w-full rounded-lg"
            />
          </div>
        </div>
        <div class="modal-action">
          <button
            class="btn btn-ghost btn-sm rounded-lg"
            @click="isEditing = false"
            :disabled="isSaving"
          >
            {{ t('actions.cancel') }}
          </button>
          <button
            class="btn btn-primary btn-sm rounded-lg"
            @click="updateProfile"
            :disabled="isSaving"
          >
            <span v-if="isSaving" class="loading loading-spinner loading-xs"></span>
            {{ t('actions.save') }}
          </button>
        </div>
      </div>
      <form method="dialog" class="modal-backdrop">
        <button @click="isEditing = false">close</button>
      </form>
    </dialog>
  </div>
</template>
