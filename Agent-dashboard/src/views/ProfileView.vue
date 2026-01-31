<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import usersApi from '@/api/users'
import type { UserQuota } from '@/api/types'
import AppIcon from '@/components/ui/AppIcon.vue'
import { formatDate } from '@/utils/format'

const authStore = useAuthStore()
const quota = ref<UserQuota | null>(null)
const isLoading = ref(false)

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
            :src="authStore.user.profile.avatar"
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
          Settings
        </button>
        <button class="btn btn-primary btn-sm rounded-lg">Edit Profile</button>
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
          <h3 class="font-semibold text-base">Personal Information</h3>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-y-6 gap-x-8">
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide"
              >Full Name</label
            >
            <p class="font-medium mt-1">
              {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
            </p>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide"
              >Email</label
            >
            <div class="flex items-center gap-2 mt-1">
              <p class="font-medium">{{ authStore.dbUser?.email }}</p>
              <span
                class="badge badge-success badge-xs bg-success/10 text-success border-0 px-2 py-2"
                >Verified</span
              >
            </div>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide"
              >Phone</label
            >
            <p class="font-medium mt-1 text-base-content/40 italic">Not provided</p>
          </div>
          <div>
            <label class="text-xs font-medium text-base-content/50 uppercase tracking-wide"
              >Joined</label
            >
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
              Usage Quota
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
              <p class="mt-4 text-xs font-medium text-base-content/50 uppercase">Monthly Limit</p>
              <p class="text-xl font-bold">{{ quota.monthlyTokenLimit.toLocaleString() }}</p>
            </div>

            <div class="space-y-3 pt-4 border-t border-base-100">
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">Used</span>
                <span class="font-medium">{{ quota.usedTokens.toLocaleString() }}</span>
              </div>
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">Remaining</span>
                <span class="font-medium text-success">{{
                  quota.remainingTokens.toLocaleString()
                }}</span>
              </div>
              <div class="flex justify-between text-sm">
                <span class="text-base-content/60">Reset Date</span>
                <span class="font-medium">{{ formatDate(quota.lastResetDate) }}</span>
              </div>
            </div>
          </template>
          <div v-else class="text-center py-8 opacity-50 text-sm">No data available</div>
        </div>

        <!-- Account Status -->
        <div class="bg-base-100 rounded-2xl p-6 border border-base-200">
          <div class="flex items-center justify-between mb-4">
            <h3 class="font-semibold text-base">Plan</h3>
            <span class="badge badge-accent badge-sm font-bold">PRO</span>
          </div>
          <p class="text-sm text-base-content/60 mb-4">
            You are on the Professional plan with priority support.
          </p>
          <button class="btn btn-sm btn-outline w-full hover:bg-base-200 hover:text-base-content">
            Manage Subscription
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
