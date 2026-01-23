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

const app = createApp(App)

// Plugins
app.use(createPinia())
app.use(router)
app.use(i18n)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      prefix: 'p',
      darkModeSelector: '.dark-mode',
      cssLayer: false,
    },
  },
})
app.use(ToastService)

app.mount('#app')
