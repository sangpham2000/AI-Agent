<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useAnalyticsStore } from '@/stores/analytics'
import AppIcon from '@/components/ui/AppIcon.vue'
import { useI18n } from 'vue-i18n'
import { formatDate } from '@/utils/format'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  CategoryScale,
  LinearScale,
} from 'chart.js'
import { Bar, Line, Doughnut } from 'vue-chartjs'

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
)

const { t } = useI18n()
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

const dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] // Should ideally be localized too, but for chart labels keep simple for now or use toLocaleDateString approach if needed.
const maxMessages = computed(() => Math.max(...analyticsStore.messagesThisWeek, 1))
const maxTrend = computed(() =>
  Math.max(...analyticsStore.conversationTrends.map((t) => t.count), 1),
)
const maxDaily = computed(() =>
  Math.max(...analyticsStore.dailyMessageCounts.map((d) => d.count), 1),
)

// --- Chart Options ---
const commonOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      padding: 12,
      titleFont: { size: 13 },
      bodyFont: { size: 13, weight: 'bold' as const },
      cornerRadius: 8,
      displayColors: false,
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: { color: 'rgba(0, 0, 0, 0.05)' },
      ticks: { font: { size: 10 } },
      border: { display: false },
    },
    x: {
      grid: { display: false },
      ticks: { font: { size: 10 } },
      border: { display: false },
    },
  },
}

// Specific options for Line Chart (smooth curves)
const lineChartOptions = {
  ...commonOptions,
  elements: {
    line: { tension: 0.4 }, // Smooth curve
    point: { radius: 0, hoverRadius: 6 },
  },
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
}

// Specific options for Doughnut
const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'right' as const, labels: { usePointStyle: true, boxWidth: 8 } },
    tooltip: commonOptions.plugins.tooltip,
  },
  cutout: '75%',
}

// --- Chart Data ---

const conversationTrendsData = computed(() => ({
  labels: analyticsStore.conversationTrends.map((t) =>
    new Date(t.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
  ),
  datasets: [
    {
      label: 'Current Period',
      data: analyticsStore.conversationTrends.map((t) => t.count),
      borderColor: '#4f46e5', // Primary
      backgroundColor: 'rgba(79, 70, 229, 0.1)',
      fill: true,
      tension: 0.4,
    },
    {
      label: 'Previous Period',
      data: analyticsStore.conversationTrends.map((t) => t.previousCount || 0),
      borderColor: '#9ca3af', // Gray-400
      backgroundColor: 'transparent',
      borderDash: [5, 5],
      pointRadius: 0,
      borderWidth: 2,
      tension: 0.4,
    },
  ],
}))

function exportToCSV() {
  const headers = ['Date', 'Conversations (Current)', 'Conversations (Previous)', 'Messages']
  const rows = analyticsStore.conversationTrends.map((t, index) => {
    const msg = analyticsStore.dailyMessageCounts[index]?.count || 0
    return [new Date(t.date).toLocaleDateString(), t.count, t.previousCount || 0, msg].join(',')
  })

  const csvContent = 'data:text/csv;charset=utf-8,' + headers.join(',') + '\n' + rows.join('\n')
  const encodedUri = encodeURI(csvContent)
  const link = document.createElement('a')
  link.setAttribute('href', encodedUri)
  link.setAttribute('download', `analytics_export_${new Date().toISOString().split('T')[0]}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const dailyMessagesData = computed(() => ({
  labels: analyticsStore.dailyMessageCounts.map((d) =>
    new Date(d.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
  ),
  datasets: [
    {
      label: 'Messages',
      data: analyticsStore.dailyMessageCounts.map((d) => d.count),
      backgroundColor: '#ec4899', // Secondary (Pink-ish)
      borderRadius: 4,
    },
  ],
}))

const platformDistributionData = computed(() => ({
  labels: analyticsStore.platformDistribution.map((p) => p.platform),
  datasets: [
    {
      data: analyticsStore.platformDistribution.map((p) => p.count),
      backgroundColor: ['#4f46e5', '#0ea5e9', '#10b981'], // Primary, Info, Success
      borderWidth: 0,
    },
  ],
}))

const weeklyPatternData = computed(() => {
  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
  // Map messagesThisWeek (last 7 days) to labels. Assuming messagesThisWeek is strictly last 7 days ending today.
  // Ideally, we should dynamically generate labels based on today.
  const labels = []
  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(d.getDate() - i)
    labels.push(d.toLocaleDateString('en-US', { weekday: 'short' }))
  }

  return {
    labels: labels,
    datasets: [
      {
        label: 'Messages',
        data: analyticsStore.messagesThisWeek,
        backgroundColor: '#10b981', // Accent/Success
        borderRadius: 4,
      },
    ],
  }
})

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
</script>

<template>
  <div class="space-y-5">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">{{ t('analytics.title') }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">
          {{ t('analytics.subtitle') }}
        </p>
      </div>
      <div class="flex gap-2">
        <div class="dropdown dropdown-end">
          <button tabindex="0" class="btn btn-ghost btn-sm rounded-lg gap-2 border border-base-300">
            {{ t('analytics.lastDays', { count: trendDays }) }}
            <AppIcon name="chevron-down" class="w-4 h-4" />
          </button>
          <ul
            tabindex="0"
            class="dropdown-content menu p-1.5 bg-base-100 rounded-xl shadow-lg border border-base-200 w-36 mt-1"
          >
            <li>
              <a class="text-sm rounded-lg" @click="changeTrendDays(7)">{{
                t('analytics.lastDays', { count: 7 })
              }}</a>
            </li>
            <li>
              <a class="text-sm rounded-lg" @click="changeTrendDays(14)">{{
                t('analytics.lastDays', { count: 14 })
              }}</a>
            </li>
            <li>
              <a class="text-sm rounded-lg" @click="changeTrendDays(30)">{{
                t('analytics.lastDays', { count: 30 })
              }}</a>
            </li>
          </ul>
        </div>

        <button
          class="btn btn-ghost btn-sm rounded-lg gap-2 border border-base-300"
          @click="exportToCSV"
        >
          <AppIcon name="download" class="w-4 h-4" />
          Export
        </button>

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
          {{ t('analytics.refresh') }}
        </button>
      </div>
    </div>

    <!-- Alerts -->
    <div v-if="analyticsStore.error" class="alert alert-error text-sm py-3 rounded-xl shadow-sm">
      <AppIcon name="exclamation" class="w-5 h-5" />
      <span>{{ analyticsStore.error }}</span>
      <button class="btn btn-ghost btn-xs" @click="analyticsStore.clearError()">
        {{ t('actions.dismiss') }}
      </button>
    </div>

    <!-- Key Metrics -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chat" class="w-3.5 h-3.5" />
          {{ t('analytics.totalConversations') }}
        </p>
        <p class="text-2xl font-bold">{{ formatCompact(analyticsStore.totalConversations) }}</p>
        <p class="text-xs text-base-content/40 mt-1">{{ t('analytics.allTime') }}</p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="calendar" class="w-3.5 h-3.5" />
          {{ t('analytics.todaysConversations') }}
        </p>
        <p class="text-2xl font-bold">{{ formatNumber(analyticsStore.conversationsToday) }}</p>
        <div class="flex items-center gap-1 mt-1">
          <span
            class="text-xs font-medium"
            :class="
              (analyticsStore.dashboardStats?.conversationGrowthRate || 0) >= 0
                ? 'text-success'
                : 'text-error'
            "
          >
            {{ (analyticsStore.dashboardStats?.conversationGrowthRate || 0) > 0 ? '+' : ''
            }}{{ analyticsStore.dashboardStats?.conversationGrowthRate || 0 }}%
          </span>
          <span class="text-xs text-base-content/40">vs last month</span>
        </div>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="mail" class="w-3.5 h-3.5" />
          {{ t('analytics.messagesThisWeek') }}
        </p>
        <p class="text-2xl font-bold">{{ formatCompact(totalMessagesThisWeek) }}</p>
        <p class="text-xs text-base-content/40 mt-1">
          {{ t('analytics.avgPerDay', { count: avgMessagesPerDay }) }}
        </p>
      </div>
      <div class="bg-base-100 rounded-2xl p-4 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="users" class="w-3.5 h-3.5" />
          {{ t('analytics.activeUsers') }}
        </p>
        <p class="text-2xl font-bold">{{ formatNumber(analyticsStore.activeUsers) }}</p>
        <p class="text-xs text-base-content/40 mt-1">
          {{ t('analytics.totalUsers', { count: analyticsStore.totalUsers }) }}
        </p>
      </div>
    </div>

    <!-- Charts Row 1 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Conversation Trends -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200 flex flex-col">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="trending-up" class="w-4 h-4 text-primary" />
            {{ t('analytics.conversationTrends') }}
          </h3>
          <div class="flex gap-2">
            <span class="badge badge-sm badge-ghost gap-1 text-[10px] text-base-content/50">
              <span
                class="w-2 h-0.5 bg-base-content/30 inline-block border-t border-dashed w-3"
              ></span>
              Previous
            </span>
            <span class="badge badge-sm badge-primary badge-outline">{{
              t('analytics.lastDays', { count: trendDays })
            }}</span>
          </div>
        </div>
        <div class="flex-1 w-full min-h-[200px] relative">
          <div
            v-if="!analyticsStore.isLoading && !analyticsStore.conversationTrends.length"
            class="absolute inset-0 flex flex-col items-center justify-center text-base-content/40"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-16 w-16 mb-2 opacity-20"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
              />
            </svg>
            <span class="text-sm font-medium">{{ t('analytics.noData') }}</span>
          </div>
          <Line v-else :data="conversationTrendsData" :options="lineChartOptions" />
        </div>
      </div>

      <!-- Daily Messages -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200 flex flex-col">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="chat-alt" class="w-4 h-4 text-secondary" />
            {{ t('analytics.dailyMessages') }}
          </h3>
          <span class="badge badge-sm badge-secondary badge-outline">{{
            t('analytics.lastDays', { count: trendDays })
          }}</span>
        </div>
        <div class="flex-1 w-full min-h-[200px] relative">
          <div
            v-if="!analyticsStore.isLoading && !analyticsStore.dailyMessageCounts.length"
            class="absolute inset-0 flex flex-col items-center justify-center text-base-content/40"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-16 w-16 mb-2 opacity-20"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
              />
            </svg>
            <span class="text-sm font-medium">{{ t('analytics.noData') }}</span>
          </div>
          <Bar v-else :data="dailyMessagesData" :options="commonOptions" />
        </div>
      </div>
    </div>

    <!-- Charts Row 2 -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Platform Distribution -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200 lg:col-span-1 flex flex-col">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="globe-alt" class="w-4 h-4 text-info" />
          {{ t('analytics.platformDistribution') }}
        </h3>
        <div class="flex-1 w-full min-h-[200px] relative flex items-center justify-center">
          <Doughnut :data="platformDistributionData" :options="doughnutOptions" />
        </div>
      </div>

      <!-- Weekly Pattern -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200 lg:col-span-2 flex flex-col">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="calendar" class="w-4 h-4 text-accent" />
            {{ t('analytics.weeklyPattern') }}
          </h3>
          <span class="badge badge-sm badge-accent badge-outline">{{
            t('analytics.thisWeek') || 'This week'
          }}</span>
        </div>
        <div class="flex-1 w-full min-h-[200px] relative">
          <Bar :data="weeklyPatternData" :options="commonOptions" />
        </div>
      </div>
    </div>

    <!-- Popular Questions -->
    <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
      <div class="flex items-center justify-between mb-4">
        <h3 class="font-semibold flex items-center gap-2">
          <AppIcon name="question-mark-circle" class="w-4 h-4 text-warning" />
          {{ t('analytics.popularQuestions') }}
        </h3>
        <div class="dropdown dropdown-end">
          <button tabindex="0" class="btn btn-ghost btn-xs gap-1">
            {{ t('analytics.topQuestions', { count: questionLimit }) }}
            <AppIcon name="chevron-down" class="w-3 h-3" />
          </button>
          <ul
            tabindex="0"
            class="dropdown-content menu p-1.5 bg-base-100 rounded-xl shadow-lg border border-base-200 w-28"
          >
            <li>
              <a class="text-sm rounded-lg" @click="changeQuestionLimit(5)">{{
                t('analytics.topQuestions', { count: 5 })
              }}</a>
            </li>
            <li>
              <a class="text-sm rounded-lg" @click="changeQuestionLimit(10)">{{
                t('analytics.topQuestions', { count: 10 })
              }}</a>
            </li>
            <li>
              <a class="text-sm rounded-lg" @click="changeQuestionLimit(20)">{{
                t('analytics.topQuestions', { count: 20 })
              }}</a>
            </li>
          </ul>
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="table table-sm">
          <thead>
            <tr class="border-base-200 bg-base-50/50">
              <th class="text-xs font-medium text-base-content/50 w-12 pl-4">#</th>
              <th class="text-xs font-medium text-base-content/50">
                {{ t('analytics.table.question') }}
              </th>
              <th class="text-xs font-medium text-base-content/50 w-20 text-right">
                {{ t('analytics.table.count') }}
              </th>
              <th class="text-xs font-medium text-base-content/50 w-28 pr-4">
                {{ t('analytics.table.lastAsked') }}
              </th>
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
                {{ t('analytics.noData') }}
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
          {{ t('analytics.documentProcessing') }}
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
          <p class="text-xs text-base-content/50">{{ t('analytics.documentsProcessed') }}</p>
        </div>
      </div>

      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="user-group" class="w-4 h-4" />
          {{ t('analytics.userActivity') }}
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
          <p class="text-xs text-base-content/50">{{ t('analytics.activeUsers') }}</p>
        </div>
      </div>

      <div class="bg-base-100 rounded-2xl p-5 border border-base-200/50">
        <h3 class="font-semibold mb-4 flex items-center gap-2">
          <AppIcon name="lightning-bolt" class="w-4 h-4" />
          {{ t('dashboard.aiPerformance') }}
        </h3>
        <div class="flex flex-col items-center">
          <p class="text-4xl font-bold text-accent">
            {{
              formatCompact(Math.round(analyticsStore.dashboardStats?.avgTokensPerResponse || 0))
            }}
          </p>
          <div
            class="flex items-center gap-1 mt-3 text-success text-sm bg-success/10 px-2 py-1 rounded-lg"
          >
            <AppIcon name="chip" class="w-3.5 h-3.5" />
            <span>tokens/response</span>
          </div>
          <p class="text-xs text-base-content/50 mt-1">Average usage</p>
        </div>
      </div>
    </div>
  </div>
</template>
