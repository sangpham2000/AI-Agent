<script setup lang="ts">
import { RouterView } from 'vue-router'
// We might want a header here later
</script>

<template>
  <div class="min-h-screen bg-base-100 flex flex-col">
    <!-- Simple Navbar -->
    <div class="navbar bg-base-100 container mx-auto px-4 py-2">
      <div class="flex-1">
        <a class="btn btn-ghost text-xl font-bold text-primary">AI Agent</a>
      </div>
      <div class="flex-none gap-2">
        <RouterLink to="/login" class="btn btn-primary btn-sm">Sign In</RouterLink>
      </div>
    </div>

    <!-- Main Content -->
    <main class="flex-1 flex flex-col">
      <RouterView />
    </main>

    <!-- Footer -->
    <footer class="footer footer-center p-4 bg-base-200 text-base-content">
      <aside>
        <p>Copyright © {{ new Date().getFullYear() }} - All right reserved by AI Agent Team</p>
      </aside>
    </footer>
  </div>
</template>
