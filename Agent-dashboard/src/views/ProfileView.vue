<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import usersApi from '@/api/users'
import type { UserQuota } from '@/api/types'
import AppIcon from '@/components/ui/AppIcon.vue'
import { formatDate } from '@/utils/format'
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

function getInitials(name: string) {
  return name ? name.charAt(0).toUpperCase() : 'U'
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
        ? dayjs(editForm.value.dateOfBirth).utc().toISOString() // Convert to UTC
        : undefined,
      lastLoginAt: authStore.dbUser.lastLoginAt
        ? dayjs(authStore.dbUser.lastLoginAt).utc().toISOString() // Ensure existing dates are also safely sent as UTC (if re-sent)
        : undefined,
    }

    // Clean up null/undefined
    if (!payload.phoneNumber) delete (payload as any).phoneNumber

    await usersApi.update(authStore.dbUser.id, payload)

    // Refresh user data
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
  <div class="space-y-6">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row items-center gap-6">
      <div class="relative">
        <div
          class="w-24 h-24 rounded-full border border-base-200 bg-base-100 flex items-center justify-center overflow-hidden"
        >
          <span v-if="!authStore.user?.profile?.avatar" class="text-3xl font-medium text-primary">
            {{ getInitials(authStore.dbUser?.firstName || '') }}
          </span>
          <img
            v-else
            :src="String(authStore.user?.profile?.avatar || '')"
            alt="Avatar"
            class="w-full h-full object-cover"
          />
        </div>
        <button
          class="btn btn-circle btn-xs btn-primary absolute bottom-0 right-0 border border-base-100 shadow-sm"
        >
          <AppIcon name="pencil" class="w-3 h-3 text-white" />
        </button>
      </div>
      <div class="text-center md:text-left">
        <h1 class="text-2xl font-bold">
          {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
        </h1>
        <p class="text-sm text-base-content/60">@{{ authStore.dbUser?.username }}</p>
        <div class="flex gap-2 mt-2 justify-center md:justify-start">
          <span
            v-for="role in authStore.userRoles"
            :key="role"
            class="badge badge-sm badge-ghost border-base-300"
          >
            {{ role }}
          </span>
        </div>
      </div>
      <div class="md:ml-auto flex gap-2">
        <button class="btn btn-ghost btn-sm rounded-lg border border-base-200">
          <AppIcon name="cog-6-tooth" class="w-4 h-4 text-base-content/70" />
          <AppIcon name="cog-6-tooth" class="w-4 h-4 text-base-content/70" />
          {{ t('profile.settings') }}
        </button>
        <button class="btn btn-primary btn-sm rounded-lg" @click="openEditModal">
          {{ t('profile.editProfile') }}
        </button>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Personal Information Card -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-6 border border-base-200">
        <div class="flex items-center gap-2 mb-6 border-b border-base-100 pb-4">
          <div class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center">
            <AppIcon name="user" class="w-4 h-4 text-primary" />
          </div>
          <h3 class="font-semibold text-base">{{ t('profile.personalInfo') }}</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-y-6 gap-x-8">
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">{{
              t('profile.fullName')
            }}</label>
            <p class="font-medium mt-1">
              {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
            </p>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">{{
              t('profile.email')
            }}</label>
            <div class="flex items-center gap-2 mt-1">
              <p class="font-medium">{{ authStore.dbUser?.email }}</p>
              <span
                class="badge badge-success badge-xs bg-success/10 text-success border-0 px-2 py-2"
                >{{ t('profile.verified') }}</span
              >
            </div>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">{{
              t('profile.phone')
            }}</label>
            <p class="font-medium mt-1 text-base-content/40 italic">
              {{ t('profile.notProvided') }}
            </p>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide">{{
              t('profile.joined')
            }}</label>
            <p class="font-medium mt-1">{{ formatDate(authStore.dbUser?.createdAt) }}</p>
          </div>
        </div>
      </div>

      <!-- Right Column -->
      <div class="space-y-6">
        <!-- Quota Status -->
        <div class="bg-base-100 rounded-2xl p-6 border border-base-200">
          <div class="flex items-center justify-between mb-6">
            <h3 class="font-semibold text-base flex items-center gap-2">
              <AppIcon name="chart-pie" class="w-4 h-4 text-secondary" />
              {{ t('profile.quota.title') }}
            </h3>
          </div>

          <div v-if="isLoading" class="flex justify-center py-8">
            <span class="loading loading-spinner text-base-content/20"></span>
          </div>

          <template v-else-if="quota">
            <div class="flex flex-col items-center mb-6">
              <div
                class="radial-progress text-primary transition-all duration-1000 bg-base-200/50 text-sm font-bold"
                :style="`--value:${quota.usagePercentage}; --size:8rem; --thickness: 0.6rem;`"
                role="progressbar"
              >
                {{ quota.usagePercentage }}%
              </div>
              <p class="mt-4 text-xs font-medium text-base-content/50 uppercase">
                {{ t('profile.quota.monthlyLimit') }}
              </p>
              <p class="text-xl font-bold">{{ quota.monthlyTokenLimit.toLocaleString() }}</p>
            </div>

            <div class="space-y-3 pt-4 border-t border-base-100">
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
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">{{ t('profile.quota.resetDate') }}</span>
                <span class="font-medium">{{ formatDate(quota.lastResetDate) }}</span>
              </div>
            </div>
          </template>
          <div v-else class="text-center py-8 opacity-50 text-sm">
            {{ t('profile.quota.noData') }}
          </div>
        </div>

        <!-- Account Status -->
        <div class="bg-base-100 rounded-2xl p-6 border border-base-200">
          <div class="flex items-center justify-between mb-4">
            <h3 class="font-semibold text-base">{{ t('profile.plan.title') }}</h3>
            <span class="badge badge-accent badge-sm font-bold">{{ t('profile.plan.pro') }}</span>
          </div>
          <p class="text-sm text-base-content/60 mb-4">
            {{ t('profile.plan.description') }}
          </p>
          <button class="btn btn-sm btn-outline w-full hover:bg-base-200 hover:text-base-content">
            {{ t('profile.plan.manage') }}
          </button>
        </div>
      </div>
    </div>

    <!-- Edit Profile Modal -->
    <dialog class="modal" :class="{ 'modal-open': isEditing }">
      <div class="modal-box">
        <h3 class="font-bold text-lg mb-4">{{ t('profile.modal.title') }}</h3>
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="form-control">
              <label class="label"
                ><span class="label-text">{{ t('profile.modal.firstName') }}</span></label
              >
              <input v-model="editForm.firstName" type="text" class="input input-bordered w-full" />
            </div>
            <div class="form-control">
              <label class="label"
                ><span class="label-text">{{ t('profile.modal.lastName') }}</span></label
              >
              <input v-model="editForm.lastName" type="text" class="input input-bordered w-full" />
            </div>
          </div>
          <div class="form-control">
            <label class="label"
              ><span class="label-text">{{ t('profile.modal.phone') }}</span></label
            >
            <input v-model="editForm.phoneNumber" type="tel" class="input input-bordered w-full" />
          </div>
          <div class="form-control">
            <label class="label"
              ><span class="label-text">{{ t('profile.modal.dob') }}</span></label
            >
            <input v-model="editForm.dateOfBirth" type="date" class="input input-bordered w-full" />
          </div>
        </div>
        <div class="modal-action">
          <button class="btn" @click="isEditing = false" :disabled="isSaving">
            {{ t('actions.cancel') }}
          </button>
          <button class="btn btn-primary" @click="updateProfile" :disabled="isSaving">
            <span v-if="isSaving" class="loading loading-spinner text-white"></span>
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
