import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('@/layouts/DashboardLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardHome.vue'),
        },
        {
          path: 'users',
          name: 'users',
          component: () => import('@/views/UsersView.vue'),
        },
        {
          path: 'documents',
          name: 'documents',
          component: () => import('@/views/DocumentsView.vue'),
        },
        {
          path: 'analytics',
          name: 'analytics',
          component: () => import('@/views/AnalyticsView.vue'),
        },
        {
          path: 'conversations',
          name: 'conversations',
          component: () => import('@/views/ConversationsView.vue'),
        },
        {
          path: 'chat',
          name: 'chat',
          component: () => import('@/views/ChatView.vue'),
        },
      ],
    },
    {
      path: '/callback',
      name: 'callback',
      component: () => import('@/views/CallbackView.vue'),
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  // We need to import the store dynamically or inside the guard to ensure Pinia is active
  const { useAuthStore } = await import('@/stores/auth')
  const authStore = useAuthStore()

  // Initialize auth store if (it tries to load user from storage)
  // We might want to ensure initialization is done only once or checked efficiently
  if (!authStore.user) {
    await authStore.initialize()
  }

  const publicPages = ['/login', '/callback']
  const authRequired = !publicPages.includes(to.path)

  if (authRequired && !authStore.isAuthenticated) {
    next('/login')
  } else {
    next()
  }
})

export default router
