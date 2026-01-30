import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'

import App from './App.vue'
import router from './router'
import i18n from './locales'

// PrimeVue styles
import 'primeicons/primeicons.css'
import './style.css'

import { sdk } from '@/sdk/AgentSDK'

const init = (
  options: {
    apiKey: string
    apiUrl?: string
    theme?: 'light' | 'dark' | 'auto'
    containerId?: string
  } = {
    apiKey: '',
  },
) => {
  const containerId = options.containerId || 'ai-agent-widget-container'
  let container = document.getElementById(containerId)

  if (!container) {
    container = document.createElement('div')
    container.id = containerId
    document.body.appendChild(container)
  }

  // Initialize SDK
  sdk.init(options)

  const app = createApp(App)

  app.use(createPinia())
  app.use(router)
  app.use(i18n)
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        prefix: 'p',
        darkModeSelector: options.theme === 'dark' ? 'html' : '.dark-mode',
        cssLayer: false,
      },
    },
  })
  app.use(ToastService)

  app.provide('widgetOptions', options)

  app.mount(container)
}

// Expose SDK to window for programmatic control
const AIAgent = {
  init,
  open: () => sdk.open(),
  close: () => sdk.close(),
  toggle: () => sdk.toggle(),
  sendMessage: (msg: string) => sdk.sendMessage(msg),
  on: (event: string, cb: any) => sdk.on(event, cb),
  off: (event: string, cb: any) => sdk.off(event, cb),
}

;(window as any).AIAgent = AIAgent
