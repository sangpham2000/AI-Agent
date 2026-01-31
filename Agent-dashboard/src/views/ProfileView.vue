<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import usersApi from '@/api/users'
import type { UserQuota } from '@/api/types'
import AppIcon from '@/components/ui/AppIcon.vue'

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

function formatDate(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}
</script>

<template>
  <div class="space-y-6 max-w-4xl mx-auto">
    <!-- Header -->
    <div>
      <h1 class="text-2xl font-bold">My Profile</h1>
      <p class="text-base-content/60">Manage your account settings and view usage.</p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <!-- User Info Card -->
      <div class="col-span-1 md:col-span-2 space-y-6">
        <div class="card bg-base-100 border border-base-200 shadow-sm">
          <div class="card-body">
            <h2 class="card-title text-base mb-4 flex items-center gap-2">
              <AppIcon name="user" class="w-5 h-5 text-primary" />
              Personal Information
            </h2>

            <div class="flex items-start gap-6">
              <div class="avatar placeholder">
                <div class="bg-neutral text-neutral-content rounded-full w-24">
                  <span class="text-3xl">{{ authStore.dbUser?.firstName[0] }}</span>
                </div>
              </div>

              <div class="space-y-4 flex-1">
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label class="label-text text-xs text-base-content/50 uppercase font-bold"
                      >Full Name</label
                    >
                    <p class="font-medium">
                      {{ authStore.dbUser?.firstName }} {{ authStore.dbUser?.lastName }}
                    </p>
                  </div>
                  <div>
                    <label class="label-text text-xs text-base-content/50 uppercase font-bold"
                      >Username</label
                    >
                    <p class="font-medium">@{{ authStore.dbUser?.username }}</p>
                  </div>
                  <div>
                    <label class="label-text text-xs text-base-content/50 uppercase font-bold"
                      >Email</label
                    >
                    <p class="font-medium">{{ authStore.dbUser?.email }}</p>
                  </div>
                  <div>
                    <label class="label-text text-xs text-base-content/50 uppercase font-bold"
                      >Role</label
                    >
                    <div class="flex gap-1 mt-1">
                      <span
                        v-for="role in authStore.userRoles"
                        :key="role"
                        class="badge badge-sm badge-secondary bg-secondary/10 border-0 text-secondary"
                      >
                        {{ role }}
                      </span>
                    </div>
                  </div>
                  <div>
                    <label class="label-text text-xs text-base-content/50 uppercase font-bold"
                      >Member Since</label
                    >
                    <p class="font-medium">{{ formatDate(authStore.dbUser?.createdAt) }}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quota Card -->
      <div class="col-span-1 space-y-6">
        <div class="card bg-base-100 border border-base-200 shadow-sm h-full">
          <div class="card-body">
            <h2 class="card-title text-base mb-4 flex items-center gap-2">
              <AppIcon name="chart-bar" class="w-5 h-5 text-secondary" />
              Usage Quota
            </h2>

            <div v-if="isLoading" class="flex justify-center py-8">
              <span class="loading loading-spinner loading-md"></span>
            </div>

            <div v-else-if="quota" class="flex flex-col items-center text-center space-y-6 py-4">
              <div
                class="radial-progress text-primary transition-all duration-1000"
                :style="`--value:${quota.usagePercentage}; --size:8rem; --thickness: 0.8rem;`"
                role="progressbar"
              >
                <div class="flex flex-col items-center">
                  <span class="text-3xl font-bold">{{ quota.usagePercentage }}%</span>
                  <span class="text-xs opacity-60">Used</span>
                </div>
              </div>

              <div class="w-full space-y-3">
                <div class="flex justify-between text-sm border-b border-base-200 pb-2">
                  <span class="text-base-content/60">Monthly Limit</span>
                  <span class="font-semibold"
                    >{{ quota.monthlyTokenLimit.toLocaleString() }} tokens</span
                  >
                </div>
                <div class="flex justify-between text-sm border-b border-base-200 pb-2">
                  <span class="text-base-content/60">Used</span>
                  <span class="font-semibold">{{ quota.usedTokens.toLocaleString() }} tokens</span>
                </div>
                <div class="flex justify-between text-sm border-b border-base-200 pb-2">
                  <span class="text-base-content/60">Remaining</span>
                  <span class="font-semibold text-success"
                    >{{ quota.remainingTokens.toLocaleString() }} tokens</span
                  >
                </div>
                <div class="flex justify-between text-sm">
                  <span class="text-base-content/60">Reset Date</span>
                  <span class="font-medium">{{
                    new Date(quota.lastResetDate).toLocaleDateString()
                  }}</span>
                </div>
              </div>

              <div v-if="quota.remainingTokens < 5000" class="alert alert-warning text-xs p-2">
                <AppIcon name="alert-triangle" class="w-4 h-4" />
                <span>You are running low on tokens.</span>
              </div>
            </div>
            <div v-else class="text-center py-8 text-base-content/50">
              User quota not available.
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
