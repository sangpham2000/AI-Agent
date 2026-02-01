<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import AppIcon from '@/components/ui/AppIcon.vue'

const { t } = useI18n()
const authStore = useAuthStore()
const router = useRouter()

// If user is already authenticated, redirect to dashboard
onMounted(() => {
  if (authStore.isAuthenticated) {
    if (authStore.isAdmin) {
      router.push({ name: 'dashboard' })
    } else {
      router.push({ name: 'chat' })
    }
  }
})

const handleLogin = () => {
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="min-h-[calc(100vh-64px)] bg-base-100 relative overflow-hidden flex flex-col">
    <!-- Dynamic Background -->
    <div class="absolute inset-0 bg-grid-pattern opacity-[0.4] z-0 pointer-events-none"></div>

    <!-- Gradient Blobs -->
    <div
      class="absolute top-[-20%] right-[-10%] w-[800px] h-[800px] bg-primary/20 rounded-full blur-3xl opacity-40 animate-pulse"
      style="animation-duration: 10s"
    ></div>
    <div
      class="absolute bottom-[-20%] left-[-10%] w-[600px] h-[600px] bg-secondary/20 rounded-full blur-3xl opacity-40 animate-pulse"
      style="animation-duration: 8s"
    ></div>

    <div
      class="flex-1 flex flex-col lg:flex-row items-center justify-center container mx-auto px-4 py-12 lg:py-20 relative z-10 gap-16"
    >
      <!-- Hero Content (Left) -->
      <div class="flex-1 max-w-2xl space-y-8 text-center lg:text-left">
        <div
          class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-base-200/50 border border-base-content/10 backdrop-blur-md text-xs font-medium text-base-content/70 mb-4"
        >
          <span class="flex h-2 w-2 relative">
            <span
              class="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary opacity-75"
            ></span>
            <span class="relative inline-flex rounded-full h-2 w-2 bg-primary"></span>
          </span>
          {{ t('landing.hero.badge') }}
        </div>

        <h1 class="text-5xl lg:text-7xl font-extrabold tracking-tight leading-[1.1]">
          {{ t('landing.hero.title') }} <br />
          <span
            class="text-transparent bg-clip-text bg-gradient-to-r from-primary via-secondary to-accent animate-gradient bg-300% text-glow"
          >
            {{ t('landing.hero.highlight') }}
          </span>
        </h1>

        <p class="text-lg text-base-content/70 leading-relaxed max-w-xl mx-auto lg:mx-0">
          {{ t('landing.hero.description') }}
        </p>

        <!-- CTA Area -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
          <button
            @click="handleLogin"
            class="btn btn-primary btn-lg rounded-xl shadow-lg shadow-primary/30 hover:shadow-primary/50 group"
          >
            {{ t('landing.cta.placeholder') }}
            <AppIcon
              name="arrow-right"
              class="w-5 h-5 group-hover:translate-x-1 transition-transform"
            />
          </button>

          <button
            class="btn btn-ghost btn-lg rounded-xl border border-base-content/10 hover:bg-base-content/5"
          >
            <AppIcon name="play" class="w-5 h-5" />
            Watch Demo
          </button>
        </div>

        <div class="flex items-center justify-center lg:justify-start gap-8 pt-4">
          <div class="flex items-center gap-2 text-sm font-medium text-base-content/60">
            <AppIcon name="shield-check" class="w-5 h-5 text-success" />
            {{ t('landing.cta.businessReady') || 'Enterprise Ready' }}
          </div>
          <div class="flex items-center gap-2 text-sm font-medium text-base-content/60">
            <AppIcon name="lightning-bolt" class="w-5 h-5 text-warning" />
            {{ t('landing.cta.fastDeploy') || 'Fast Deployment' }}
          </div>
        </div>
      </div>

      <!-- Hero Visual (Right) - 3D Mockup -->
      <div class="flex-1 w-full max-w-xl perspective-1000 hidden lg:block">
        <div class="relative transform-3d rotate-y-12 animate-float">
          <!-- Glass Mockup Card -->
          <div
            class="bg-base-100/80 backdrop-blur-xl border border-white/20 dark:border-white/10 rounded-2xl shadow-2xl overflow-hidden p-4 relative z-10"
          >
            <!-- Fake Browser Header -->
            <div class="flex items-center gap-2 mb-4 border-b border-base-content/5 pb-2">
              <div class="flex gap-1.5">
                <div class="w-3 h-3 rounded-full bg-error/80"></div>
                <div class="w-3 h-3 rounded-full bg-warning/80"></div>
                <div class="w-3 h-3 rounded-full bg-success/80"></div>
              </div>
              <div class="flex-1 mx-4 bg-base-200/50 rounded-md h-6"></div>
            </div>

            <!-- Content Mockup -->
            <div class="space-y-4">
              <div class="flex gap-4">
                <!-- Sidebar -->
                <div class="w-1/4 space-y-2">
                  <div class="h-8 bg-primary/10 rounded-lg w-full"></div>
                  <div class="h-8 bg-base-200/50 rounded-lg w-3/4"></div>
                  <div class="h-8 bg-base-200/50 rounded-lg w-4/5"></div>
                </div>
                <!-- Main -->
                <div class="flex-1 space-y-4">
                  <!-- Chat Bubbles -->
                  <div class="flex justify-end">
                    <div
                      class="bg-primary text-primary-content p-3 rounded-2xl rounded-tr-none max-w-[80%] text-sm shadow-sm"
                    >
                      Generate a quarterly report for Q3 performance.
                    </div>
                  </div>
                  <div class="flex justify-start">
                    <div
                      class="bg-base-200 p-3 rounded-2xl rounded-tl-none max-w-[90%] text-sm shadow-sm space-y-2"
                    >
                      <div class="font-semibold text-xs opacity-50">AI Agent</div>
                      <p>Here is the summary for Q3:</p>
                      <div
                        class="h-24 bg-base-100 rounded-lg border border-base-300/50 flex items-end justify-around p-2 gap-1"
                      >
                        <div class="w-full bg-primary/40 h-[40%] rounded-sm"></div>
                        <div class="w-full bg-primary/60 h-[70%] rounded-sm"></div>
                        <div class="w-full bg-primary/80 h-[50%] rounded-sm"></div>
                        <div class="w-full bg-primary h-[90%] rounded-sm"></div>
                      </div>
                    </div>
                  </div>

                  <!-- Input Mockup -->
                  <div class="pt-4">
                    <div
                      class="h-10 border border-primary/30 rounded-full flex items-center px-4 gap-2"
                    >
                      <div class="w-4 h-4 rounded-full bg-primary/20 animate-pulse"></div>
                      <div class="h-2 w-20 bg-base-content/10 rounded-full"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Decorative Elements behind mockup -->
          <div
            class="absolute -right-10 -bottom-10 w-40 h-40 bg-accent/30 rounded-full blur-2xl z-0"
          ></div>
          <div
            class="absolute -left-10 -top-10 w-40 h-40 bg-primary/30 rounded-full blur-2xl z-0"
          ></div>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
/* Optional subtle animation for the search bar text placeholder if needed in future */
</style>
