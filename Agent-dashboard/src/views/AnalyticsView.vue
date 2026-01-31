<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useAnalyticsStore } from '@/stores/analytics'
import AppIcon from '@/components/ui/AppIcon.vue'

const analyticsStore = useAnalyticsStore()

const trendDays = ref(30)
const questionLimit = ref(10)

onMounted(async () => {
  analyticsStore.isLoading = true
  try {
    await Promise.all([
      analyticsStore.fetchDashboardStats(),
      analyticsStore.fetchConversationTrends(trendDays.value),
      analyticsStore.fetchPopularQuestions(questionLimit.value),
      analyticsStore.fetchDailyMessageCounts(trendDays.value),
      analyticsStore.fetchPlatformDistribution(),
    ])
  } finally {
    analyticsStore.isLoading = false
  }
})

const formatNumber = (num: number) => num.toLocaleString()
const formatCompact = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M'
  if (num >= 1000) return (num / 1000).toFixed(1) + 'K'
  return num.toString()
}

const totalMessagesThisWeek = computed(() => {
  return analyticsStore.messagesThisWeek.reduce((a, b) => a + b, 0)
})

const avgMessagesPerDay = computed(() => {
  const weeks = analyticsStore.messagesThisWeek
  if (weeks.length === 0) return 0
  return Math.round(weeks.reduce((a, b) => a + b, 0) / weeks.length)
})

const dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
const maxMessages = computed(() => Math.max(...analyticsStore.messagesThisWeek, 1))
const maxTrend = computed(() =>
  Math.max(...analyticsStore.conversationTrends.map((t) => t.count), 1),
)
const maxDaily = computed(() =>
  Math.max(...analyticsStore.dailyMessageCounts.map((d) => d.count), 1),
)

async function refreshData() {
  await analyticsStore.fetchAll()
}

async function changeTrendDays(days: number) {
  trendDays.value = days
  await analyticsStore.fetchConversationTrends(days)
  await analyticsStore.fetchDailyMessageCounts(days)
}

async function changeQuestionLimit(limit: number) {
  questionLimit.value = limit
  await analyticsStore.fetchPopularQuestions(limit)
}

import { formatDate } from '@/utils/format'
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">Analytics Dashboard</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          Detailed insights into your AI Agent performance.
        </p>
      </div>
      <div class="flex gap-2">
        <div class="dropdown dropdown-end">
          <button tabindex="0" class="btn btn-ghost btn-sm rounded-lg gap-2">
            Last {{ trendDays }} days
            <AppIcon name="chevron-down" class="w-4 h-4" />
          </button>
          <ul
            tabindex="0"
            class="dropdown-content menu p-1.5 bg-base-100 rounded-xl shadow-lg border border-base-200 w-36 mt-1"
          >
            <li><a class="text-sm rounded-lg" @click="changeTrendDays(7)">Last 7 days</a></li>
            <li><a class="text-sm rounded-lg" @click="changeTrendDays(14)">Last 14 days</a></li>
            <li><a class="text-sm rounded-lg" @click="changeTrendDays(30)">Last 30 days</a></li>
          </ul>
        </div>
        <button
          class="btn btn-primary btn-sm rounded-lg gap-2"
          @click="refreshData"
          :disabled="analyticsStore.isLoading"
        >
          <AppIcon
            name="refresh"
            class="w-4 h-4"
            :class="{ 'animate-spin': analyticsStore.isLoading }"
          />
          Refresh
        </button>
      </div>
    </div>

    <!-- Alerts -->
    <div v-if="analyticsStore.error" class="alert alert-error text-sm py-3 rounded-xl shadow-sm">
      <AppIcon name="exclamation" class="w-5 h-5" />
      <span>{{ analyticsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="analyticsStore.clearError()">Dismiss</button>
    </div>

    <!-- Key Metrics -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chat" class="w-3.5 h-3.5" />
          Total Conversations
        </p>
        <p class="text-2xl font-bold">{{ formatCompact(analyticsStore.totalConversations) }}</p>
        <p class="text-xs text-base-content/40 mt-1">All time</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="calendar" class="w-3.5 h-3.5" />
          Today's Conversations
        </p>
        <p class="text-2xl font-bold">{{ formatNumber(analyticsStore.conversationsToday) }}</p>
        <p class="text-xs text-success mt-1">+12% vs yesterday</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="mail" class="w-3.5 h-3.5" />
          Messages This Week
        </p>
        <p class="text-2xl font-bold">{{ formatCompact(totalMessagesThisWeek) }}</p>
        <p class="text-xs text-base-content/40 mt-1">~{{ avgMessagesPerDay }}/day avg</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="users" class="w-3.5 h-3.5" />
          Active Users
        </p>
        <p class="text-2xl font-bold">{{ formatNumber(analyticsStore.activeUsers) }}</p>
        <p class="text-xs text-base-content/40 mt-1">of {{ analyticsStore.totalUsers }} total</p>
      </div>
    </div>

    <!-- Charts Row 1 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Conversation Trends -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="trending-up" class="w-4 h-4 text-primary" />
            Conversation Trends
          </h3>
          <span class="badge badge-sm badge-primary badge-outline">{{ trendDays }} days</span>
        </div>
        <div v-if="analyticsStore.isLoading" class="h-48 flex items-center justify-center">
          <span class="loading loading-spinner loading-md text-primary"></span>
        </div>
        <div v-else class="h-48 flex items-end gap-1">
          <div
            v-for="(trend, index) in analyticsStore.conversationTrends"
            :key="index"
            class="flex-1 flex flex-col items-center gap-1 group relative"
          >
            <!-- Tooltip -->
            <div
              class="absolute -top-8 bg-base-300 text-[10px] py-1 px-2 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-10"
            >
              {{ trend.count }} conversations
            </div>
            <div
              class="w-full bg-primary/20 hover:bg-primary rounded-t transition-all"
              :style="{ height: `${Math.max((trend.count / maxTrend) * 100, 4)}%` }"
            ></div>
          </div>
        </div>
        <div
          v-if="!analyticsStore.isLoading && !analyticsStore.conversationTrends.length"
          class="h-48 flex items-center justify-center text-base-content/40 text-sm"
        >
          No trend data available
        </div>
      </div>

      <!-- Daily Messages -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="chat-alt" class="w-4 h-4 text-secondary" />
            Daily Messages
          </h3>
          <span class="badge badge-sm badge-secondary badge-outline">{{ trendDays }} days</span>
        </div>
        <div v-if="analyticsStore.isLoading" class="h-48 flex items-center justify-center">
          <span class="loading loading-spinner loading-md text-secondary"></span>
        </div>
        <div v-else class="h-48 flex items-end gap-1">
          <div
            v-for="(day, index) in analyticsStore.dailyMessageCounts"
            :key="index"
            class="flex-1 flex flex-col items-center gap-1 group relative"
          >
            <!-- Tooltip -->
            <div
              class="absolute -top-8 bg-base-300 text-[10px] py-1 px-2 rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-10"
            >
              {{ day.count }} messages
            </div>
            <div
              class="w-full bg-secondary/20 hover:bg-secondary rounded-t transition-all"
              :style="{ height: `${Math.max((day.count / maxDaily) * 100, 4)}%` }"
            ></div>
          </div>
        </div>
        <div
          v-if="!analyticsStore.isLoading && !analyticsStore.dailyMessageCounts.length"
          class="h-48 flex items-center justify-center text-base-content/40 text-sm"
        >
          No message data available
        </div>
      </div>
    </div>

    <!-- Charts Row 2 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Platform Distribution -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="globe-alt" class="w-4 h-4 text-info" />
          Conversations by Platform
        </h3>
        <div v-if="analyticsStore.isLoading" class="space-y-3">
          <div v-for="i in 3" :key="i" class="h-10 bg-base-200 rounded-lg animate-pulse"></div>
        </div>
        <div v-else class="space-y-4">
          <div
            v-for="(platform, index) in analyticsStore.platformDistribution"
            :key="platform.platform"
            class="space-y-1.5"
          >
            <div class="flex items-center justify-between text-sm">
              <span class="capitalize flex items-center gap-2">
                <AppIcon
                  v-if="platform.platform === 'web'"
                  name="desktop-computer"
                  class="w-4 h-4 text-primary"
                />
                <AppIcon
                  v-else-if="platform.platform === 'telegram'"
                  name="paper-airplane"
                  class="w-4 h-4 text-info"
                />
                <AppIcon v-else name="terminal" class="w-4 h-4 text-accent" />
                {{ platform.platform }}
              </span>
              <span class="font-medium"
                >{{ platform.count }}
                <span class="text-xs text-base-content/50"
                  >({{
                    ((platform.count / (analyticsStore.totalConversations || 1)) * 100).toFixed(1)
                  }}%)</span
                ></span
              >
            </div>
            <div class="h-2 bg-base-200 rounded-full overflow-hidden">
              <div
                :class="[
                  'h-full rounded-full',
                  index === 0 ? 'bg-primary' : index === 1 ? 'bg-info' : 'bg-secondary',
                ]"
                :style="{
                  width: `${(platform.count / (analyticsStore.totalConversations || 1)) * 100}%`,
                }"
              ></div>
            </div>
          </div>
          <div
            v-if="!analyticsStore.platformDistribution.length"
            class="text-center py-8 text-base-content/40 text-sm"
          >
            No platform data available
          </div>
        </div>
      </div>

      <!-- Weekly Pattern -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="calendar" class="w-4 h-4 text-accent" />
            Weekly Message Pattern
          </h3>
          <span class="badge badge-sm badge-accent badge-outline">This week</span>
        </div>
        <div class="h-40 flex items-end justify-between gap-2">
          <div
            v-for="(count, index) in analyticsStore.messagesThisWeek"
            :key="index"
            class="flex-1 flex flex-col items-center gap-1"
          >
            <span class="text-[10px] font-medium text-base-content/50">{{ count }}</span>
            <div
              class="w-full bg-accent/20 hover:bg-accent rounded-t transition-all"
              :style="{ height: `${Math.max((count / maxMessages) * 100, 8)}%` }"
            ></div>
            <span class="text-[10px] text-base-content/40">{{ dayLabels[index] }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Popular Questions -->
    <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
      <div class="flex items-center justify-between mb-4">
        <h3 class="font-semibold flex items-center gap-2">
          <AppIcon name="question-mark-circle" class="w-4 h-4 text-warning" />
          Popular Questions
        </h3>
        <div class="dropdown dropdown-end">
          <button tabindex="0" class="btn btn-ghost btn-xs gap-1">
            Top {{ questionLimit }}
            <AppIcon name="chevron-down" class="w-3 h-3" />
          </button>
          <ul
            tabindex="0"
            class="dropdown-content menu p-1.5 bg-base-100 rounded-xl shadow-lg border border-base-200 w-28"
          >
            <li><a class="text-sm rounded-lg" @click="changeQuestionLimit(5)">Top 5</a></li>
            <li><a class="text-sm rounded-lg" @click="changeQuestionLimit(10)">Top 10</a></li>
            <li><a class="text-sm rounded-lg" @click="changeQuestionLimit(20)">Top 20</a></li>
          </ul>
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="table table-sm">
          <thead>
            <tr class="border-base-200 bg-base-50/50">
              <th class="text-xs font-medium text-base-content/50 w-12 pl-4">#</th>
              <th class="text-xs font-medium text-base-content/50">Question</th>
              <th class="text-xs font-medium text-base-content/50 w-20 text-right">Count</th>
              <th class="text-xs font-medium text-base-content/50 w-28 pr-4">Last Asked</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="analyticsStore.isLoading">
              <td colspan="4" class="text-center py-8">
                <span class="loading loading-spinner loading-sm text-primary"></span>
              </td>
            </tr>
            <tr v-else-if="!analyticsStore.popularQuestions.length">
              <td colspan="4" class="text-center py-8 text-base-content/40 text-sm">
                No questions recorded yet
              </td>
            </tr>
            <tr
              v-for="(question, index) in analyticsStore.popularQuestions"
              :key="index"
              class="hover border-base-100"
            >
              <td class="pl-4">
                <span
                  :class="[
                    'badge badge-sm font-semibold border-0 w-6 h-6 p-0 flex items-center justify-center',
                    index === 0
                      ? 'bg-primary text-primary-content'
                      : index === 1
                        ? 'bg-secondary text-secondary-content'
                        : index === 2
                          ? 'bg-accent text-accent-content'
                          : 'bg-base-200 text-base-content',
                  ]"
                >
                  {{ index + 1 }}
                </span>
              </td>
              <td class="max-w-md">
                <p class="text-sm line-clamp-1 font-medium">{{ question.question }}</p>
              </td>
              <td class="text-right font-semibold text-sm">{{ question.count }}</td>
              <td class="text-sm text-base-content/50 pr-4">
                {{ formatDate(question.lastAsked) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Document & User Stats -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="document-text" class="w-4 h-4" />
          Document Processing
        </h3>
        <div class="flex flex-col items-center">
          <div
            class="radial-progress text-primary text-xl font-bold"
            :style="{
              '--value': analyticsStore.totalDocuments
                ? (analyticsStore.documentsProcessed / analyticsStore.totalDocuments) * 100
                : 0,
              '--size': '7rem',
              '--thickness': '0.75rem',
            }"
            role="progressbar"
          >
            {{
              analyticsStore.totalDocuments
                ? Math.round(
                    (analyticsStore.documentsProcessed / analyticsStore.totalDocuments) * 100,
                  )
                : 0
            }}%
          </div>
          <p class="mt-3 text-sm font-medium">
            {{ analyticsStore.documentsProcessed }} / {{ analyticsStore.totalDocuments }}
          </p>
          <p class="text-xs text-base-content/50">Documents Processed</p>
        </div>
      </div>

      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="user-group" class="w-4 h-4" />
          User Activity
        </h3>
        <div class="flex flex-col items-center">
          <div
            class="radial-progress text-success text-xl font-bold"
            :style="{
              '--value': analyticsStore.totalUsers
                ? (analyticsStore.activeUsers / analyticsStore.totalUsers) * 100
                : 0,
              '--size': '7rem',
              '--thickness': '0.75rem',
            }"
            role="progressbar"
          >
            {{
              analyticsStore.totalUsers
                ? Math.round((analyticsStore.activeUsers / analyticsStore.totalUsers) * 100)
                : 0
            }}%
          </div>
          <p class="mt-3 text-sm font-medium">
            {{ analyticsStore.activeUsers }} / {{ analyticsStore.totalUsers }}
          </p>
          <p class="text-xs text-base-content/50">Active Users</p>
        </div>
      </div>

      <div class="bg-base-100 rounded-2xl p-5 border border-base-200/50">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="lightning-bolt" class="w-4 h-4" />
          Avg Response Time
        </h3>
        <div class="flex flex-col items-center">
          <p class="text-4xl font-bold text-accent">1.2s</p>
          <div
            class="flex items-center gap-1 mt-3 text-success text-sm bg-success/10 px-2 py-1 rounded-lg"
          >
            <AppIcon name="trending-up" class="w-3.5 h-3.5" />
            <span>15% faster</span>
          </div>
          <p class="text-xs text-base-content/50 mt-1">vs last week</p>
        </div>
      </div>
    </div>
  </div>
</template>
