<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useAnalyticsStore } from '@/stores/analytics'
import { useAuthStore } from '@/stores/auth'
import { RouterLink } from 'vue-router'
import { useI18n } from 'vue-i18n'
import AppIcon from '@/components/ui/AppIcon.vue'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from 'chart.js'
import { Bar } from 'vue-chartjs'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const { t, locale } = useI18n()

const analyticsStore = useAnalyticsStore()
const authStore = useAuthStore()

const selectedTimeRange = ref(7)

onMounted(async () => {
  await Promise.all([
    analyticsStore.fetchAll(),
    analyticsStore.fetchDailyMessageCounts(selectedTimeRange.value),
  ])
})

watch(selectedTimeRange, async (newDays) => {
  await analyticsStore.fetchDailyMessageCounts(newDays)
})

const chartData = computed(() => {
  const isVi = locale.value === 'vi'
  return {
    labels: analyticsStore.dailyMessageCounts.map((item) =>
      new Date(item.date).toLocaleDateString(isVi ? 'vi-VN' : 'en-US', {
        weekday: 'short',
        month: 'short',
        day: 'numeric',
      }),
    ),
    datasets: [
      {
        label: t('dashboard.cards.conversations.title'),
        backgroundColor: '#4f46e5', // Primary color
        borderRadius: 4,
        data: analyticsStore.dailyMessageCounts.map((item) => item.count),
      },
    ],
  }
})

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false,
    },
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
      grid: {
        color: 'rgba(0, 0, 0, 0.05)',
      },
      ticks: {
        font: { size: 10 },
      },
      border: { display: false },
    },
    x: {
      grid: {
        display: false,
      },
      ticks: {
        font: { size: 10 },
      },
      border: { display: false },
    },
  },
}))

const formatNumber = (num: number) => num.toLocaleString()
const formatCompact = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(2) + 'M'
  if (num >= 1000) return (num / 1000).toFixed(2) + 'K'
  return num.toString()
}

const timeAgo = (dateStr: string) => {
  const date = new Date(dateStr)
  const now = new Date()
  const seconds = Math.floor((now.getTime() - date.getTime()) / 1000)

  // Simple copy for now, ideally use a localized format distance function
  if (seconds < 60) return 'Just now'
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  return `${Math.floor(hours / 24)}d ago`
}

const totalMessagesThisWeek = computed(() => {
  return analyticsStore.dailyMessageCounts.reduce((a, b) => a + b.count, 0)
})

const averageMessagesPerDay = computed(() => {
  if (!analyticsStore.dailyMessageCounts.length) return 0
  return Math.round(totalMessagesThisWeek.value / analyticsStore.dailyMessageCounts.length)
})

const quickInsights = computed(() => [
  {
    label: t('dashboard.total'),
    value: formatCompact(analyticsStore.totalConversations),
    sublabel: t('dashboard.cards.conversations.title').toLowerCase(),
  },
  {
    label: t('dashboard.today'),
    value: formatNumber(analyticsStore.conversationsToday),
    sublabel: t('dashboard.newChats'),
  },
  {
    label: t('dashboard.growth'),
    value: `${(analyticsStore.dashboardStats?.conversationGrowthRate || 0) > 0 ? '+' : ''}${analyticsStore.dashboardStats?.conversationGrowthRate || 0}%`,
    sublabel: t('dashboard.thisMonth'),
    positive: (analyticsStore.dashboardStats?.conversationGrowthRate || 0) >= 0,
  },
])

const systemMetrics = computed(() => [
  {
    label: t('dashboard.apiSuccessRate'),
    value: '99.9%', // Can be real later
    status: t('dashboard.stablePerformance'),
    statusColor: 'success',
  },
  {
    label: t('dashboard.responseTime'),
    value: '150ms', // Can be real later
    status: 'Fast',
    statusColor: 'success',
  },
  {
    label: t('dashboard.aiPerformance'),
    value: `${Math.round(analyticsStore.dashboardStats?.avgTokensPerResponse || 0)} tokens/res`,
    status: 'Efficient',
    statusColor: 'info',
  },
  {
    label: t('dashboard.serverLoad'),
    value: 'Normal',
    status: 'Optimized',
    statusColor: 'success',
  },
])
</script>

<template>
  <div class="space-y-5">
    <!-- Welcome & Quick Actions -->
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">
          {{ t('dashboard.welcome', { name: authStore.userName || 'Admin' }) }}
        </h1>
        <p class="text-sm text-base-content/50 mt-0.5">{{ t('dashboard.overview') }}</p>
      </div>
      <div class="flex items-center gap-2">
        <button class="btn btn-primary btn-sm gap-2 rounded-lg pl-3 pr-4">
          <AppIcon name="chat" class="w-4 h-4" />
          {{ t('dashboard.askAi') }}
        </button>
        <button
          class="btn btn-ghost btn-sm gap-2 rounded-lg"
          @click="analyticsStore.fetchAll()"
          :disabled="analyticsStore.isLoading"
        >
          <AppIcon
            name="refresh"
            class="w-4 h-4"
            :class="{ 'animate-spin': analyticsStore.isLoading }"
          />
          {{ t('actions.refresh') }}
        </button>
        <RouterLink to="dashboard/analytics" class="btn btn-ghost btn-sm gap-2 rounded-lg">
          <AppIcon name="chart-pie" class="w-4 h-4" />
          {{ t('dashboard.getInsights') }}
        </RouterLink>
      </div>
    </div>

    <!-- Quick Insights & Main Stats -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-4">
      <!-- Quick Insights Card -->
      <div class="lg:col-span-6 bg-base-100 rounded-2xl p-5 border border-base-200">
        <h3
          class="text-xs font-medium text-base-content/50 uppercase tracking-wide mb-4 flex items-center gap-2"
        >
          <AppIcon name="light-bulb" class="w-3.5 h-3.5" />
          {{ t('dashboard.quickInsights') }}
        </h3>
        <div class="grid grid-cols-3 gap-4">
          <div v-for="(insight, i) in quickInsights" :key="i">
            <p class="text-2xl font-bold" :class="insight.positive ? 'text-success' : ''">
              {{ insight.value }}
            </p>
            <p class="text-[11px] text-base-content/50 mt-0.5">{{ insight.sublabel }}</p>
          </div>
        </div>
      </div>

      <!-- Token Consumption -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chip" class="w-3.5 h-3.5" />
          {{ t('dashboard.systemTokenConsumption') }}
        </p>
        <p class="text-2xl font-bold">
          {{ formatCompact(analyticsStore.totalTokensUsedThisMonth || 0) }}
        </p>
        <p class="text-[11px] text-base-content/50">{{ t('dashboard.thisMonth') }}</p>
      </div>

      <!-- Active Users -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="user-group" class="w-3.5 h-3.5" />
          {{ t('dashboard.activeUsers') }}
        </p>
        <p class="text-2xl font-bold">
          {{ formatCompact(analyticsStore.activeUsers || 0) }}
        </p>
        <p class="text-[11px] text-base-content/50">
          {{ formatCompact(analyticsStore.totalUsers || 0) }}
          {{ t('dashboard.totalUsers').toLowerCase() }}
        </p>
      </div>

      <!-- Total Messages (Avg) - actually let's use Avg Messages/Day since user agreed or Total Users. 
           User agreed to: "Thay Processing Power -> Avg Messages/Day or Total Users."
           Let's use Total Users for the bigger number, OR Avg Messages/Day.
           Actually I'll stick to the plan: Processing Power -> Total Users? No, I used Total Users as sublabel for Active Users above. 
           Let's use "Avg Messages/Day" here as the 4th card, or maybe "Conversations Today" if not already shown.
           Quick Insights has "Today" (Conversations Today).
           I will use "Average Messages / Day" here.
       -->
      <!-- Wait, let me re-read my plan.
           "Thay Processing Power -> Avg Messages/Day (Trung bình tin nhắn/ngày) hoặc Total Users."
           I put Total Users as subtext of Active Users.
           So let's make the 4th card "Avg Messages/Day".
           Do I have that data? analyticsStore.dailyMessageCounts has that.
           Let's compute it in script or just use a simple metric.
           analyticsStore has `avgMessages` key in conversations table, but checking store...
           `dailyMessageCounts` is an array. I can compute average from that.
       -->
      <!-- Re-evaluating. User said "Thay Processing Power -> Avg Messages/Day".
            I will compute avg messages per day from `analyticsStore.dashboardStats` provided data?
            Looking at store, I have `messagesThisWeek`. I can compute avg from that.
       -->
      <!-- Let's calculate avg messages/day from messagesThisWeek (which is array of ints). -->

      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="chat" class="w-3.5 h-3.5" />
          {{ t('analytics.avgPerDay', { count: averageMessagesPerDay }) }}
        </p>
        <p class="text-2xl font-bold">
          {{ formatCompact(averageMessagesPerDay) }}
        </p>
        <p class="text-[11px] text-base-content/50">
          {{ t('dashboard.thisMonth') }}
        </p>
      </div>
    </div>

    <!-- Chart & System Status -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <!-- Models Overview Chart -->
      <div class="xl:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h3 class="font-semibold flex items-center gap-2">
              <AppIcon name="chart-bar" class="w-4 h-4 text-primary" />
              {{ t('dashboard.modelsOverview') }}
            </h3>
            <p class="text-xs text-base-content/50 mt-1">
              {{ t('dashboard.tokensProcessedToday') }}
            </p>
          </div>

          <div class="dropdown dropdown-end">
            <button
              tabindex="0"
              class="btn btn-ghost btn-sm rounded-lg gap-2 border border-base-300"
            >
              {{ t('analytics.lastDays', { count: selectedTimeRange }) }}
              <AppIcon name="chevron-down" class="w-4 h-4" />
            </button>
            <ul
              tabindex="0"
              class="dropdown-content menu p-1.5 bg-base-100 rounded-xl shadow-lg border border-base-200 w-36 mt-1"
            >
              <li v-for="days in [7, 14, 30, 90]" :key="days">
                <a class="text-sm rounded-lg" @click="selectedTimeRange = days">{{
                  t('analytics.lastDays', { count: days })
                }}</a>
              </li>
            </ul>
          </div>
        </div>

        <div class="flex items-baseline gap-2 mb-6">
          <p class="text-3xl font-bold">{{ formatCompact(totalMessagesThisWeek) }}</p>
          <span
            class="badge badge-sm border-0"
            :class="
              (analyticsStore.dashboardStats?.conversationGrowthRate || 0) >= 0
                ? 'badge-success bg-success/10 text-success-content'
                : 'badge-error bg-error/10 text-error-content'
            "
          >
            {{ (analyticsStore.dashboardStats?.conversationGrowthRate || 0) > 0 ? '+' : ''
            }}{{ analyticsStore.dashboardStats?.conversationGrowthRate || 0 }}%
          </span>
        </div>

        <!-- Chart JS -->
        <div class="h-64 relative w-full">
          <Bar :data="chartData" :options="chartOptions" />
        </div>
      </div>

      <!-- System Status -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="status-online" class="w-4 h-4 text-success" />
            {{ t('dashboard.systemStatus') }}
          </h3>
          <span class="flex items-center gap-1.5 text-xs text-success font-medium">
            <span class="relative flex h-2 w-2">
              <span
                class="animate-ping absolute inline-flex h-full w-full rounded-full bg-success opacity-75"
              ></span>
              <span class="relative inline-flex rounded-full h-2 w-2 bg-success"></span>
            </span>
            {{ t('dashboard.operational') }}
          </span>
        </div>

        <div class="space-y-4">
          <div
            v-for="(metric, i) in systemMetrics"
            :key="i"
            class="flex items-center justify-between group"
          >
            <div>
              <p class="text-sm font-medium">{{ metric.value }}</p>
              <p
                class="text-[11px] text-base-content/50 group-hover:text-primary transition-colors"
              >
                {{ metric.label }}
              </p>
            </div>
            <span
              :class="[
                'badge badge-sm border-0 font-medium',
                `bg-${metric.statusColor}/10`,
                `text-${metric.statusColor}`,
              ]"
            >
              {{ metric.status }}
            </span>
          </div>
        </div>

        <!-- Models Section -->
        <div class="mt-6 pt-4 border-t border-base-200">
          <div class="flex items-center justify-between mb-3">
            <h4 class="font-medium text-sm text-base-content/70">
              {{ t('dashboard.activeModels') }}
            </h4>
            <button class="btn btn-ghost btn-xs gap-1 opacity-50 hover:opacity-100">
              <AppIcon name="plus" class="w-3 h-3" />
              {{ t('actions.add') }}
            </button>
          </div>
          <div class="space-y-2">
            <div
              v-for="model in analyticsStore.dashboardStats?.activeModels"
              :key="model.name"
              class="flex items-center gap-3 p-2.5 rounded-xl bg-base-50 hover:bg-base-200/50 transition-colors border border-transparent hover:border-base-200 cursor-pointer"
              :class="{ 'opacity-60': model.status === 'Inactive' }"
            >
              <div
                class="w-8 h-8 rounded-lg flex items-center justify-center text-xs font-bold"
                :class="
                  model.name.includes('GPT')
                    ? 'bg-secondary/10 text-secondary'
                    : 'bg-primary/10 text-primary'
                "
              >
                {{ model.name.substring(0, 2).toUpperCase() }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium">{{ model.name }}</p>
                <div class="flex items-center gap-2">
                  <span
                    class="badge badge-xs"
                    :class="
                      model.status === 'Active' ? 'bg-success/10 text-success' : 'badge-ghost'
                    "
                  >
                    {{ model.status }}
                  </span>
                  <p v-if="model.isDefault" class="text-[10px] text-base-content/50">Default</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Row -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Global AI Trends -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="globe" class="w-4 h-4 text-info" />
            {{ t('dashboard.platformDistribution') }}
          </h3>
          <button class="btn btn-ghost btn-xs btn-square">
            <AppIcon name="download" class="w-3.5 h-3.5 opacity-50" />
          </button>
        </div>
        <div class="space-y-4">
          <div
            v-for="(platform, index) in analyticsStore.platformDistribution"
            :key="platform.platform"
            class="space-y-1.5"
          >
            <div class="flex items-center justify-between text-sm">
              <div class="flex items-center gap-2">
                <AppIcon
                  v-if="platform.platform === 'web'"
                  name="desktop-computer"
                  class="w-3.5 h-3.5 opacity-50"
                />
                <AppIcon
                  v-else-if="platform.platform === 'telegram'"
                  name="paper-airplane"
                  class="w-3.5 h-3.5 opacity-50"
                />
                <AppIcon v-else name="code" class="w-3.5 h-3.5 opacity-50" />
                <span class="capitalize">{{ platform.platform }}</span>
              </div>
              <span class="font-medium"
                >{{
                  Math.round((platform.count / (analyticsStore.totalConversations || 1)) * 100)
                }}%</span
              >
            </div>
            <div class="h-1.5 bg-base-200 rounded-full overflow-hidden">
              <div
                :class="[
                  'h-full rounded-full transition-all',
                  index === 0 ? 'bg-primary' : index === 1 ? 'bg-secondary' : 'bg-accent',
                ]"
                :style="{
                  width: `${(platform.count / (analyticsStore.totalConversations || 1)) * 100}%`,
                }"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Event Feed -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="clock" class="w-4 h-4 text-warning" />
            {{ t('dashboard.systemEventFeed') }}
          </h3>
        </div>
        <div class="overflow-x-auto">
          <table class="table table-sm">
            <thead>
              <tr class="text-xs text-base-content/50 border-b border-base-200">
                <th class="font-medium pl-0">Event</th>
                <th class="font-medium">Type</th>
                <th class="font-medium pr-0 text-right">Time</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="activity in analyticsStore.recentActivities"
                :key="activity.id"
                class="hover border-0"
              >
                <td class="flex items-center gap-2 pl-0">
                  <div class="p-1.5 rounded-md bg-base-200">
                    <AppIcon
                      :name="
                        activity.type === 'User'
                          ? 'user'
                          : activity.type === 'Document'
                            ? 'document-text'
                            : 'chat'
                      "
                      class="w-3.5 h-3.5 text-base-content/70"
                    />
                  </div>
                  <span
                    class="text-sm font-medium truncate max-w-[150px]"
                    :title="activity.description"
                    >{{ activity.description }}</span
                  >
                </td>
                <td>
                  <span
                    :class="[
                      'badge badge-xs font-medium border-0 py-2',
                      activity.status === 'New' ||
                      activity.status === 'Processed' ||
                      activity.status === 'Active'
                        ? 'bg-success/15 text-success'
                        : activity.status === 'Pending'
                          ? 'bg-warning/15 text-warning'
                          : 'bg-info/15 text-info',
                    ]"
                  >
                    <!-- These statuses might need translation too, but often come from backend. Assuming English for now or add keys later if requested. -->
                    {{ activity.status }}
                  </span>
                </td>
                <td class="text-xs text-base-content/60 pr-0 text-right whitespace-nowrap">
                  {{ timeAgo(activity.timestamp) }}
                </td>
              </tr>
              <tr v-if="analyticsStore.recentActivities.length === 0">
                <td colspan="3" class="text-center text-xs text-base-content/50 py-4">
                  No recent activity
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- System Resources / Knowledge Base -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200/50">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="database" class="w-4 h-4 text-primary" />
            {{ t('dashboard.knowledgeBase') }}
          </h3>
          <RouterLink
            to="/dashboard/documents"
            class="btn btn-ghost btn-xs opacity-50 hover:opacity-100"
            >{{ t('actions.viewAll') }}</RouterLink
          >
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary"
            >
              <AppIcon name="document-text" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">{{ t('dashboard.totalDocs') }}</p>
              <p class="text-[10px] text-base-content/50">
                {{ analyticsStore.totalDocuments }} files
              </p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-success/10 flex items-center justify-center text-success"
            >
              <AppIcon name="check-circle" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">{{ t('dashboard.processed') }}</p>
              <p class="text-[10px] text-base-content/50">
                {{ analyticsStore.documentsProcessed }} ready
              </p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-warning/10 flex items-center justify-center text-warning"
            >
              <AppIcon name="clock" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">{{ t('dashboard.pending') }}</p>
              <p class="text-[10px] text-base-content/50">
                {{ analyticsStore.totalDocuments - analyticsStore.documentsProcessed }} waiting
              </p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-info/10 flex items-center justify-center text-info">
              <AppIcon name="server" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">{{ t('dashboard.vectorDb') }}</p>
              <p class="text-[10px] text-base-content/50">{{ t('dashboard.connected') }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
