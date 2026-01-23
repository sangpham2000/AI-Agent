import { createI18n } from 'vue-i18n';
import en from './en';
import vi from './vi';

// Get saved locale from localStorage or use default 'en'
const savedLocale = localStorage.getItem('locale') || 'en';

export const i18n = createI18n({
  legacy: false, // Use Composition API
  locale: savedLocale,
  fallbackLocale: 'en',
  messages: {
    en,
    vi,
  },
});

// Helper function to change locale
export function setLocale(locale: 'en' | 'vi') {
  i18n.global.locale.value = locale;
  localStorage.setItem('locale', locale);
  document.documentElement.setAttribute('lang', locale);
}

// Helper function to get current locale
export function getLocale(): string {
  return i18n.global.locale.value;
}

export default i18n;
