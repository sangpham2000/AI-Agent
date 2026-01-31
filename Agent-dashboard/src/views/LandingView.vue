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
  <div
    class="min-h-[calc(100vh-64px)] bg-base-100 relative overflow-hidden flex flex-col items-center justify-center"
  >
    <!-- Background Decor (Gradient Blobs) -->
    <div
      class="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] bg-primary/20 rounded-full blur-3xl opacity-60 animate-pulse"
    ></div>
    <div
      class="absolute bottom-[-10%] right-[-10%] w-[500px] h-[500px] bg-secondary/20 rounded-full blur-3xl opacity-60 animate-pulse"
      style="animation-duration: 4s"
    ></div>
    <div
      class="absolute top-[20%] right-[10%] w-[300px] h-[300px] bg-accent/20 rounded-full blur-3xl opacity-50"
    ></div>

    <div class="hero-content text-center relative z-10 max-w-5xl w-full flex-col gap-12 py-12">
      <!-- Hero Section -->
      <div class="max-w-3xl space-y-8">
        <div class="space-y-4">
          <div class="badge badge-primary badge-outline mb-4">{{ t('landing.hero.badge') }}</div>
          <h1
            class="text-5xl md:text-7xl font-extrabold tracking-tight text-base-content leading-tight"
          >
            {{ t('landing.hero.title') }} <br />
            <span
              class="text-transparent bg-clip-text bg-gradient-to-r from-primary to-secondary"
              >{{ t('landing.hero.highlight') }}</span
            >
          </h1>
          <p class="py-4 text-lg md:text-xl text-base-content/60 max-w-2xl mx-auto leading-relaxed">
            {{ t('landing.hero.description') }}
          </p>
        </div>

        <!-- Fake Search Bar (Call to Action) -->
        <div
          @click="handleLogin"
          class="group w-full max-w-2xl mx-auto bg-base-100 border border-base-300 rounded-full shadow-lg hover:shadow-xl hover:border-primary/50 transition-all duration-300 cursor-pointer p-2 pr-2 flex items-center gap-4 relative overflow-hidden"
        >
          <div
            class="absolute inset-0 bg-base-200/50 group-hover:bg-transparent transition-colors"
          ></div>

          <div
            class="w-10 h-10 rounded-full bg-base-200 flex items-center justify-center relative z-10 group-hover:bg-primary/10 group-hover:text-primary transition-colors"
          >
            <AppIcon
              name="sparkles"
              class="w-5 h-5 text-base-content/50 group-hover:text-primary"
            />
          </div>

          <div class="flex-1 text-left relative z-10">
            <span
              class="text-base-content/40 text-lg group-hover:text-base-content/60 transition-colors"
              >{{ t('landing.cta.placeholder') }}</span
            >
          </div>

          <button class="btn btn-primary btn-circle btn-sm relative z-10">
            <AppIcon name="arrow-right" class="w-4 h-4" />
          </button>
        </div>

        <div class="flex items-center justify-center gap-6 text-sm text-base-content/50">
          <span class="flex items-center gap-1"
            ><AppIcon name="check" class="w-4 h-4 text-success" />
            {{ t('landing.cta.multiTenant') }}</span
          >
          <span class="flex items-center gap-1"
            ><AppIcon name="check" class="w-4 h-4 text-success" />
            {{ t('landing.cta.enterprise') }}</span
          >
        </div>
      </div>

      <!-- Feature Grid -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 w-full text-left mt-8">
        <div
          class="card bg-base-100/50 border border-base-200 backdrop-blur-sm hover:-translate-y-1 hover:shadow-md transition-all duration-300"
        >
          <div class="card-body">
            <div class="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center mb-2">
              <AppIcon name="cube" class="w-5 h-5 text-primary" />
            </div>
            <h3 class="font-bold text-lg">{{ t('landing.features.core.title') }}</h3>
            <p class="text-base-content/60 text-sm">
              {{ t('landing.features.core.description') }}
            </p>
          </div>
        </div>

        <div
          class="card bg-base-100/50 border border-base-200 backdrop-blur-sm hover:-translate-y-1 hover:shadow-md transition-all duration-300"
        >
          <div class="card-body">
            <div class="w-10 h-10 rounded-lg bg-secondary/10 flex items-center justify-center mb-2">
              <AppIcon name="database" class="w-5 h-5 text-secondary" />
            </div>
            <h3 class="font-bold text-lg">{{ t('landing.features.knowledge.title') }}</h3>
            <p class="text-base-content/60 text-sm">
              {{ t('landing.features.knowledge.description') }}
            </p>
          </div>
        </div>

        <div
          class="card bg-base-100/50 border border-base-200 backdrop-blur-sm hover:-translate-y-1 hover:shadow-md transition-all duration-300"
        >
          <div class="card-body">
            <div class="w-10 h-10 rounded-lg bg-accent/10 flex items-center justify-center mb-2">
              <AppIcon name="lightning-bolt" class="w-5 h-5 text-accent" />
            </div>
            <h3 class="font-bold text-lg">{{ t('landing.features.scale.title') }}</h3>
            <p class="text-base-content/60 text-sm">
              {{ t('landing.features.scale.description') }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Optional subtle animation for the search bar text placeholder if needed in future */
</style>
