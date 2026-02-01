<script setup lang="ts">
import { RouterView, RouterLink } from 'vue-router'
import AppIcon from '@/components/ui/AppIcon.vue'
import AppLogo from '@/components/ui/AppLogo.vue'
import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const handleRegister = () => {
  const authority =
    import.meta.env.VITE_KEYCLOAK_AUTHORITY || 'http://localhost:8180/realms/ai-agent'
  const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'ai-agent-app'
  const redirectUri = `${window.location.origin}/callback`

  const signUpUrl = `${authority}/protocol/openid-connect/registrations?client_id=${clientId}&response_type=code&scope=openid%20profile%20email&redirect_uri=${encodeURIComponent(redirectUri)}`

  window.location.href = signUpUrl
}
</script>

<template>
  <div class="min-h-screen bg-base-100 flex flex-col font-sans">
    <!-- Premium Glass Grid Navbar -->
    <nav
      class="sticky top-0 z-50 border-b border-base-200/50 bg-base-100/80 backdrop-blur-xl supports-[backdrop-filter]:bg-base-100/60"
    >
      <div class="container mx-auto px-6 h-16 flex items-center justify-between">
        <!-- Logo -->
        <RouterLink to="/" class="flex items-center gap-2 group">
          <div
            class="w-8 h-8 rounded-xl bg-gradient-to-tr from-primary to-secondary flex items-center justify-center text-white shadow-lg shadow-primary/20 group-hover:shadow-primary/40 transition-all duration-300"
          >
            <AppLogo class="w-5 h-5 text-white" />
          </div>
          <span
            class="text-lg font-bold bg-clip-text text-transparent bg-gradient-to-r from-base-content to-base-content/70"
          >
            AI Agent
          </span>
        </RouterLink>

        <!-- Desktop Navigation -->
        <div class="hidden md:flex items-center gap-8">
          <RouterLink
            to="/features"
            class="text-sm font-medium text-base-content/60 hover:text-primary transition-colors"
            active-class="text-primary font-semibold"
            >{{ t('landing.nav.features') || 'Features' }}</RouterLink
          >
          <RouterLink
            to="/solutions"
            class="text-sm font-medium text-base-content/60 hover:text-primary transition-colors"
            active-class="text-primary font-semibold"
            >{{ t('landing.nav.solutions') || 'Solutions' }}</RouterLink
          >
          <RouterLink
            to="/pricing"
            class="text-sm font-medium text-base-content/60 hover:text-primary transition-colors"
            active-class="text-primary font-semibold"
            >{{ t('landing.nav.pricing') || 'Pricing' }}</RouterLink
          >
          <RouterLink
            to="/resources"
            class="text-sm font-medium text-base-content/60 hover:text-primary transition-colors"
            active-class="text-primary font-semibold"
            >{{ t('landing.nav.resources') || 'Resources' }}</RouterLink
          >
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-4">
          <RouterLink
            to="/login"
            class="hidden sm:flex text-sm font-medium text-base-content/70 hover:text-primary transition-colors"
          >
            {{ t('landing.nav.signIn') || 'Sign In' }}
          </RouterLink>

          <button
            @click="handleRegister"
            class="hidden sm:flex btn btn-primary btn-sm rounded-full px-6 font-medium text-white shadow-lg shadow-primary/20 hover:shadow-primary/40 hover:-translate-y-0.5 transition-all duration-300"
          >
            {{ t('landing.nav.signUp') || 'Sign Up' }}
          </button>

          <!-- Language Switcher -->
          <LanguageSwitcher />

          <!-- Theme Toggle -->
          <label class="swap swap-rotate btn btn-ghost btn-circle btn-sm">
            <input type="checkbox" class="theme-controller" value="light" />

            <!-- sun icon -->
            <svg
              class="swap-on fill-current w-5 h-5 text-warning"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
            >
              <path
                d="M5.64,17l-.71.71a1,1,0,0,0,0,1.41,1,1,0,0,0,1.41,0l.71-.71A1,1,0,0,0,5.64,17ZM5,12a1,1,0,0,0-1-1H3a1,1,0,0,0,0,2H4A1,1,0,0,0,5,12Zm7-7a1,1,0,0,0,1-1V3a1,1,0,0,0-2,0V4A1,1,0,0,0,12,5ZM5.64,7.05a1,1,0,0,0,.7.29,1,1,0,0,0,.71-.29,1,1,0,0,0,0-1.41l-.71-.71A1,1,0,0,0,4.93,6.34Zm12,.29a1,1,0,0,0,.7-.29l.71-.71a1,1,0,1,0-1.41-1.41L17,5.64a1,1,0,0,0,0,1.41A1,1,0,0,0,17.66,7.34ZM21,11H20a1,1,0,0,0,0,2h1a1,1,0,0,0,0-2Zm-9,8a1,1,0,0,0-1,1v1a1,1,0,0,0,2,0V20A1,1,0,0,0,12,19ZM18.36,17A1,1,0,0,0,17,18.36l.71.71a1,1,0,0,0,1.41,0,1,1,0,0,0,0-1.41ZM12,6.5A5.5,5.5,0,1,0,17.5,12,5.51,5.51,0,0,0,12,6.5Zm0,9A3.5,3.5,0,1,1,15.5,12,3.5,3.5,0,0,1,12,15.5Z"
              />
            </svg>

            <!-- moon icon -->
            <svg
              class="swap-off fill-current w-5 h-5 text-base-content/70"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
            >
              <path
                d="M21.64,13a1,1,0,0,0-1.05-.14,8.05,8.05,0,0,1-3.37.73A8.15,8.15,0,0,1,9.08,5.49a8.59,8.59,0,0,1,.25-2A1,1,0,0,0,8,2.36,10.14,10.14,0,1,0,22,14.05,1,1,0,0,0,21.64,13Zm-9.5,6.69A8.14,8.14,0,0,1,7.08,5.22v.27A10.15,10.15,0,0,0,17.22,15.63a9.79,9.79,0,0,0,2.1-.22A8.11,8.11,0,0,1,12.14,19.73Z"
              />
            </svg>
          </label>
        </div>
      </div>
    </nav>

    <!-- Main Content -->
    <main class="flex-1 flex flex-col relative">
      <RouterView />
    </main>

    <!-- Modern Footer -->
    <footer class="bg-base-100 border-t border-base-200 pt-16 pb-8">
      <div class="container mx-auto px-6">
        <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-8 mb-12">
          <div class="col-span-2 lg:col-span-2">
            <div class="flex items-center gap-2 mb-4">
              <div
                class="w-6 h-6 rounded-lg bg-primary/20 flex items-center justify-center text-primary"
              >
                <AppIcon name="sparkles" class="w-4 h-4" />
              </div>
              <span class="font-bold text-lg">AI Agent</span>
            </div>
            <p class="text-base-content/60 text-sm max-w-xs leading-relaxed">
              Empowering organizations with intelligent, scalable, and secure AI agents. The future
              of automation is here.
            </p>
          </div>

          <div>
            <h4 class="font-bold text-sm mb-4">Product</h4>
            <ul class="space-y-2 text-sm text-base-content/60">
              <li>
                <RouterLink to="/features" class="hover:text-primary transition-colors"
                  >Features</RouterLink
                >
              </li>
              <li><a href="#" class="hover:text-primary transition-colors">Integrations</a></li>
              <li>
                <RouterLink to="/pricing" class="hover:text-primary transition-colors"
                  >Pricing</RouterLink
                >
              </li>
              <li><a href="#" class="hover:text-primary transition-colors">Changelog</a></li>
            </ul>
          </div>

          <div>
            <h4 class="font-bold text-sm mb-4">Resources</h4>
            <ul class="space-y-2 text-sm text-base-content/60">
              <li>
                <RouterLink to="/resources" class="hover:text-primary transition-colors"
                  >Documentation</RouterLink
                >
              </li>
              <li>
                <RouterLink to="/resources" class="hover:text-primary transition-colors"
                  >API Reference</RouterLink
                >
              </li>
              <li>
                <RouterLink to="/resources" class="hover:text-primary transition-colors"
                  >Community</RouterLink
                >
              </li>
              <li>
                <RouterLink to="/resources" class="hover:text-primary transition-colors"
                  >Blog</RouterLink
                >
              </li>
            </ul>
          </div>

          <div>
            <h4 class="font-bold text-sm mb-4">Company</h4>
            <ul class="space-y-2 text-sm text-base-content/60">
              <li><a href="#" class="hover:text-primary transition-colors">About</a></li>
              <li><a href="#" class="hover:text-primary transition-colors">Careers</a></li>
              <li><a href="#" class="hover:text-primary transition-colors">Legal</a></li>
              <li><a href="#" class="hover:text-primary transition-colors">Contact</a></li>
            </ul>
          </div>
        </div>

        <div
          class="border-t border-base-200 pt-8 flex flex-col md:flex-row items-center justify-between gap-4"
        >
          <p class="text-xs text-base-content/40">
            © {{ new Date().getFullYear() }} AI Agent Platform. All rights reserved.
          </p>
          <div class="flex gap-4">
            <a href="#" class="text-base-content/40 hover:text-primary transition-colors">
              <span class="sr-only">Twitter</span>
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path
                  d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.713v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84"
                />
              </svg>
            </a>
            <a href="#" class="text-base-content/40 hover:text-primary transition-colors">
              <span class="sr-only">GitHub</span>
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path
                  fill-rule="evenodd"
                  d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z"
                  clip-rule="evenodd"
                />
              </svg>
            </a>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>
