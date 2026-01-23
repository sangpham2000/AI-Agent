export default {
  // Common
  app: {
    name: 'AI Agent',
    title: 'Educational AI Assistant',
  },

  // Navigation
  nav: {
    home: 'Home',
    login: 'Login',
    logout: 'Logout',
    profile: 'Profile',
  },

  // Login page
  login: {
    title: 'Login',
    subtitle: 'Use your SSO account to login to the system',
    button: 'Login with SSO',
    redirecting: 'Redirecting...',
    error: 'Login failed. Please try again.',
  },

  // Dashboard
  dashboard: {
    welcome: 'Hello, {name}',
    greeting: 'Welcome back!',
    cards: {
      analytics: {
        title: 'Analytics',
        description: 'View metrics and statistics',
      },
      documents: {
        title: 'Documents',
        description: 'Manage documents',
      },
      conversations: {
        title: 'Conversations',
        description: 'View chat history',
      },
      users: {
        title: 'Users',
        description: 'Manage users (Admin)',
      },
    },
  },

  // Chat
  chat: {
    placeholder: 'Type your question...',
    send: 'Send',
    welcome: 'Hello! I am your AI educational assistant.',
    askToStart: 'Ask a question to get started.',
    typing: 'Typing...',
    error: 'Sorry, an error occurred. Please try again.',
    loginRequired: 'Please login to use the chat',
  },

  // Auth
  auth: {
    processing: 'Processing login...',
    callbackError: 'Authentication failed',
  },

  // Common actions
  actions: {
    save: 'Save',
    cancel: 'Cancel',
    delete: 'Delete',
    edit: 'Edit',
    create: 'Create',
    upload: 'Upload',
    download: 'Download',
    search: 'Search',
    filter: 'Filter',
    refresh: 'Refresh',
  },

  // Common messages
  messages: {
    loading: 'Loading...',
    success: 'Success!',
    error: 'An error occurred',
    noData: 'No data available',
    confirm: 'Are you sure?',
  },

  // Language
  language: {
    en: 'English',
    vi: 'Vietnamese',
    switch: 'Switch language',
  },
};
