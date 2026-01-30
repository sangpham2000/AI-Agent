<script setup lang="ts">
import { computed } from 'vue'
import MarkdownIt from 'markdown-it'
import highlightjs from 'highlight.js'
import 'highlight.js/styles/github-dark.css' // Import a theme

const props = defineProps<{
  content: string
  role: 'user' | 'assistant' | 'system'
  timestamp: number
}>()

const md = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true,
  highlight: function (str, lang) {
    if (lang && highlightjs.getLanguage(lang)) {
      try {
        return highlightjs.highlight(str, { language: lang }).value
      } catch (__) {}
    }
    return '' // use external default escaping
  },
})

const renderedContent = computed(() => {
  return md.render(props.content)
})

const formatTime = (ts: number) => {
  return new Date(ts).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
</script>

<template>
  <div class="flex flex-col mb-4" :class="role === 'user' ? 'items-end' : 'items-start'">
    <div
      class="max-w-[80%] rounded-lg p-3 text-sm shadow-sm"
      :class="[
        role === 'user'
          ? 'bg-primary text-primary-content rounded-br-none'
          : 'bg-base-200 text-base-content rounded-bl-none',
      ]"
    >
      <div v-html="renderedContent" class="prose prose-sm max-w-none dark:prose-invert"></div>
    </div>
    <span class="text-xs text-base-content/50 mt-1 px-1">
      {{ formatTime(timestamp) }}
    </span>
  </div>
</template>

<style>
/* Scoped styles for prose if needed, but Tailwind Typography plugin is best if available. 
   Since we authorized tailwind/daisyui, we assume standard classes or simple overrides.
*/
.prose p {
  margin-bottom: 0.5em;
}
.prose p:last-child {
  margin-bottom: 0;
}
.prose pre {
  background-color: #282c34;
  border-radius: 0.375rem;
  padding: 0.75rem;
  overflow-x: auto;
  color: #abb2bf;
}
</style>
