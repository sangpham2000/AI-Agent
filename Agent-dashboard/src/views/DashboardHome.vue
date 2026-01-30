<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useAnalyticsStore } from '@/stores/analytics'
import { useAuthStore } from '@/stores/auth'
import { RouterLink } from 'vue-router'
import AppIcon from '@/components/ui/AppIcon.vue'

const analyticsStore = useAnalyticsStore()
const authStore = useAuthStore()

onMounted(async () => {
  await analyticsStore.fetchAll()
})

const formatNumber = (num: number) => num.toLocaleString()
const formatCompact = (num: number) => {
  if (num >= 1000000) return (num / 1000000).toFixed(2) + 'M'
  if (num >= 1000) return (num / 1000).toFixed(2) + 'K'
  return num.toString()
}

const totalMessagesThisWeek = computed(() => {
  return analyticsStore.messagesThisWeek.reduce((a, b) => a + b, 0)
})

const dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
const maxMessages = computed(() => Math.max(...analyticsStore.messagesThisWeek, 1))

const quickInsights = computed(() => [
  {
    label: 'Total',
    value: formatCompact(analyticsStore.totalConversations),
    sublabel: 'conversations',
  },
  { label: 'Today', value: formatNumber(analyticsStore.conversationsToday), sublabel: 'new chats' },
  { label: 'Growth', value: '+18.4%', sublabel: 'this month', positive: true },
])

const systemMetrics = [
  { label: 'API Success Rate', value: '98%', status: 'Stable', statusColor: 'success' },
  { label: 'Response Time', value: '200ms', status: 'Acceptable', statusColor: 'success' },
  { label: 'AI Performance', value: '350 tokens/req', status: 'Efficient', statusColor: 'info' },
  { label: 'Server Load', value: '75%', status: 'High Load', statusColor: 'warning' },
]

const recentTasks = [
  { name: 'Model Fine-Tuning', status: 'In Progress', duration: '2h 30m', icon: 'lightning-bolt' },
  { name: 'Dataset Processing', status: 'On Hold', duration: '1h 15m', icon: 'chart-bar' },
  { name: 'Generating AI Art', status: 'Done', duration: '45m', icon: 'color-swatch' },
  { name: 'Running Inference', status: 'In Progress', duration: '5h 10m', icon: 'rocket' },
]
</script>

<template>
  <div class="space-y-5">
    <!-- Welcome & Quick Actions -->
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold">Welcome Back, {{ authStore.userName || 'Admin' }}</h1>
        <p class="text-sm text-base-content/50 mt-0.5">Here's an overview of insights</p>
      </div>
      <div class="flex items-center gap-2">
        <button class="btn btn-primary btn-sm gap-2 rounded-lg pl-3 pr-4">
          <AppIcon name="chat" class="w-4 h-4" />
          Ask AI
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
          Refresh
        </button>
        <RouterLink to="dashboard/analytics" class="btn btn-ghost btn-sm gap-2 rounded-lg">
          <AppIcon name="chart-pie" class="w-4 h-4" />
          Get Insights
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
          Quick Insights
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
          Token Consumption
        </p>
        <p class="text-2xl font-bold">{{ formatCompact(totalMessagesThisWeek * 150) }}</p>
        <p class="text-[11px] text-base-content/50">Higher than usual</p>
      </div>

      <!-- AI Model Accuracy -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="check-circle" class="w-3.5 h-3.5" />
          Average Accuracy
        </p>
        <p class="text-2xl font-bold">97.2%</p>
        <p class="text-[11px] text-base-content/50">Stable performance</p>
      </div>

      <!-- Processing Power -->
      <div class="lg:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <p class="text-xs text-base-content/50 mb-1 flex items-center gap-1.5">
          <AppIcon name="server" class="w-3.5 h-3.5" />
          Processing Power
        </p>
        <p class="text-2xl font-bold">96 <span class="text-sm font-normal">TFLOPS</span></p>
        <p class="text-[11px] text-base-content/50">Performance at peak</p>
      </div>
    </div>

    <!-- Chart & System Status -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <!-- Models Overview Chart -->
      <div class="xl:col-span-2 bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-6">
          <div>
            <h3 class="font-semibold flex items-center gap-2">
              <AppIcon name="presentation-chart" class="w-4 h-4 text-primary" />
              Models Overview
            </h3>
            <div class="flex items-baseline gap-2 mt-1">
              <p class="text-3xl font-bold">{{ formatCompact(totalMessagesThisWeek * 100) }}</p>
              <span class="badge badge-sm badge-success bg-success/10 border-0">+3.14%</span>
            </div>
            <p class="text-xs text-base-content/50">Tokens processed today</p>
          </div>
          <div class="flex items-center gap-4 text-xs">
            <select class="select select-sm select-bordered rounded-lg">
              <option>This Month</option>
              <option>Last Month</option>
            </select>
          </div>
        </div>

        <!-- Simple line chart representation -->
        <div class="h-48 flex items-end gap-1 pt-4 relative">
          <div
            v-for="(count, index) in analyticsStore.messagesThisWeek"
            :key="index"
            class="flex-1 flex flex-col items-center group relative"
          >
            <div
              class="w-full bg-primary/80 rounded-t-sm transition-all group-hover:bg-primary group-hover:scale-y-105 origin-bottom"
              :style="{ height: `${Math.max((count / maxMessages) * 100, 8)}%` }"
            ></div>
            <!-- Tooltip -->
            <div
              class="absolute -top-8 bg-base-300 text-[10px] py-1 px-2 rounded opacity-0 group-hover:opacity-100 transition-opacity"
            >
              {{ formatNumber(count) }}
            </div>
          </div>
          <!-- X-axis labels -->
          <div
            class="absolute -bottom-5 left-0 right-0 flex justify-between text-[10px] text-base-content/40 px-2"
          >
            <span v-for="day in dayLabels" :key="day">{{ day }}</span>
          </div>
        </div>
      </div>

      <!-- System Status -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="status-online" class="w-4 h-4 text-success" />
            System Status
          </h3>
          <span class="flex items-center gap-1.5 text-xs text-success font-medium">
            <span class="relative flex h-2 w-2">
              <span
                class="animate-ping absolute inline-flex h-full w-full rounded-full bg-success opacity-75"
              ></span>
              <span class="relative inline-flex rounded-full h-2 w-2 bg-success"></span>
            </span>
            Operational
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
            <h4 class="font-medium text-sm text-base-content/70">Active Models</h4>
            <button class="btn btn-ghost btn-xs gap-1 opacity-50 hover:opacity-100">
              <AppIcon name="plus" class="w-3 h-3" />
              Add
            </button>
          </div>
          <div class="space-y-2">
            <div
              class="flex items-center gap-3 p-2.5 rounded-xl bg-base-50 hover:bg-base-200/50 transition-colors border border-transparent hover:border-base-200 cursor-pointer"
            >
              <div
                class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary text-xs font-bold"
              >
                N7
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium">NeuroX-7</p>
                <div class="flex items-center gap-2">
                  <progress
                    class="progress progress-primary w-16 h-1"
                    value="70"
                    max="100"
                  ></progress>
                  <p class="text-[10px] text-base-content/50">95k reqs</p>
                </div>
              </div>
            </div>
            <div
              class="flex items-center gap-3 p-2.5 rounded-xl bg-base-50 hover:bg-base-200/50 transition-colors border border-transparent hover:border-base-200 cursor-pointer"
            >
              <div
                class="w-8 h-8 rounded-lg bg-secondary/10 flex items-center justify-center text-secondary text-xs font-bold"
              >
                SM
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium">SynthaMind-5</p>
                <div class="flex items-center gap-2">
                  <progress
                    class="progress progress-secondary w-16 h-1"
                    value="45"
                    max="100"
                  ></progress>
                  <p class="text-[10px] text-base-content/50">72k reqs</p>
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
            Platform Distribution
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

      <!-- Recent Generations -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="clock" class="w-4 h-4 text-warning" />
            Recent Activity
          </h3>
        </div>
        <div class="overflow-x-auto">
          <table class="table table-sm">
            <thead>
              <tr class="text-xs text-base-content/50 border-b border-base-200">
                <th class="font-medium pl-0">Task</th>
                <th class="font-medium">Status</th>
                <th class="font-medium pr-0 text-right">Time</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="task in recentTasks" :key="task.name" class="hover border-0">
                <td class="flex items-center gap-2 pl-0">
                  <div class="p-1.5 rounded-md bg-base-200">
                    <AppIcon :name="task.icon" class="w-3.5 h-3.5 text-base-content/70" />
                  </div>
                  <span class="text-sm font-medium">{{ task.name }}</span>
                </td>
                <td>
                  <span
                    :class="[
                      'badge badge-xs font-medium border-0 py-2',
                      task.status === 'Done'
                        ? 'bg-success/15 text-success'
                        : task.status === 'In Progress'
                          ? 'bg-info/15 text-info'
                          : 'bg-warning/15 text-warning',
                    ]"
                  >
                    {{ task.status }}
                  </span>
                </td>
                <td class="text-xs text-base-content/60 pr-0 text-right">{{ task.duration }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Resource Utilization -->
      <div class="bg-base-100 rounded-2xl p-5 border border-base-200/50">
        <div class="flex items-center justify-between mb-4">
          <h3 class="font-semibold flex items-center gap-2">
            <AppIcon name="database" class="w-4 h-4 text-error" />
            Resources
          </h3>
          <RouterLink to="/analytics" class="btn btn-ghost btn-xs opacity-50 hover:opacity-100"
            >View All</RouterLink
          >
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-info/10 flex items-center justify-center text-info">
              <AppIcon name="cloud" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">EchoWave</p>
              <p class="text-[10px] text-base-content/50">Online</p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-success/10 flex items-center justify-center text-success"
            >
              <AppIcon name="microphone" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">SynthVoice</p>
              <p class="text-[10px] text-base-content/50">Active</p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary"
            >
              <AppIcon name="brain" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">Cortex</p>
              <p class="text-[10px] text-base-content/50">Processing</p>
            </div>
          </div>
          <div class="p-3 rounded-xl bg-base-50 border border-base-100 flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-warning/10 flex items-center justify-center text-warning"
            >
              <AppIcon name="document-text" class="w-4 h-4" />
            </div>
            <div>
              <p class="text-xs font-semibold">DeepScribe</p>
              <p class="text-[10px] text-base-content/50">Idle</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
