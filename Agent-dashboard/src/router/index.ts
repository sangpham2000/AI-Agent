import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('@/layouts/LandingLayout.vue'),
      children: [
        {
          path: '',
          name: 'landing',
          component: () => import('@/views/LandingView.vue'),
        },
        {
          path: 'features',
          name: 'features',
          component: () => import('@/views/FeaturesView.vue'),
        },
        {
          path: 'solutions',
          name: 'solutions',
          component: () => import('@/views/SolutionsView.vue'),
        },
        {
          path: 'pricing',
          name: 'pricing',
          component: () => import('@/views/PricingView.vue'),
        },
        {
          path: 'resources',
          name: 'resources',
          component: () => import('@/views/ResourcesView.vue'),
        },
      ],
    },
    {
      path: '/dashboard',
      component: () => import('@/layouts/DashboardLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardHome.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'users',
          name: 'users',
          component: () => import('@/views/UsersView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'roles',
          name: 'roles',
          component: () => import('@/views/RolesView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'documents',
          name: 'documents',
          component: () => import('@/views/DocumentsView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'analytics',
          name: 'analytics',
          component: () => import('@/views/AnalyticsView.vue'),
          meta: { requiresAdmin: true },
        },
        {
          path: 'conversations',
          name: 'conversations',
          component: () => import('@/views/ConversationsView.vue'),
          meta: { requiresAdmin: true },
        },

        {
          path: 'profile',
          name: 'profile',
          component: () => import('@/views/ProfileView.vue'),
        },
      ],
    },
    {
      path: '/chat',
      component: () => import('@/layouts/ChatLayout.vue'),
      children: [
        {
          path: '',
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

  const publicPages = [
    '/',
    '/login',
    '/callback',
    '/features',
    '/solutions',
    '/pricing',
    '/resources',
  ]
  const authRequired = !publicPages.includes(to.path)

  if (authRequired && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.matched.some((record) => record.meta.requiresAdmin)) {
    // Check for admin role
    if (authStore.isAdmin) {
      next()
    } else {
      // Not authorized, redirect to landing or show error
      // Ideally redirect to a dedicated "Unauthorized" page or stay on landing
      next('/')
    }
  } else {
    next()
  }
})

export default router
