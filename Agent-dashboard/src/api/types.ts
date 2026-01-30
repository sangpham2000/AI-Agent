// Analytics API Types
export interface DashboardStats {
  totalConversations: number
  conversationsToday: number
  totalDocuments: number
  documentsProcessed: number
  totalUsers: number
  activeUsers: number
  messagesThisWeek: number[]
  conversationsByPlatform: PlatformDistribution[]
}

export interface PlatformDistribution {
  platform: string
  count: number
}

export interface ConversationTrend {
  date: string
  count: number
}

export interface PopularQuestion {
  question: string
  count: number
  lastAsked: string
}

export interface DailyMessageCount {
  date: string
  count: number
}

// User API Types
export interface User {
  id: string
  username: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  dateOfBirth?: string
  avatarUrl?: string
  isActive: boolean
  lastLoginAt?: string
  createdAt: string
  roles: string[]
}

export interface CreateUser {
  username: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  dateOfBirth?: string
  avatarUrl?: string
}

export interface UpdateUser extends CreateUser {
  isActive: boolean
}

export interface Permission {
  id: string
  code: string
  name: string
  description: string
  group: string
}

export interface Role {
  id: string
  name: string
  description: string
  permissions: Permission[]
}

export interface UserPermissions {
  isAdmin: boolean
  isSuperAdmin: boolean
  roles: string[]
}

// Document API Types
export interface Document {
  id: string
  title: string
  fileName: string
  fileType: string
  category?: string
  description?: string
  fileSize: number
  isProcessed: boolean
  chunkCount: number
  createdAt: string
  updatedAt: string
}

export interface DocumentDetail extends Document {
  processingError?: string
  uploadedByUserId?: string
}

export interface ListDocumentsResponse {
  items: Document[]
  totalCount: number
  page: number
  pageSize: number
}

// Conversation API Types
export interface ConversationSummary {
  id: string
  userId?: string
  userName?: string
  userEmail?: string
  platform: string
  messageCount: number
  startedAt: string
  endedAt?: string
  status: string
}

export interface ListConversationsResponse {
  items: ConversationSummary[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface Message {
  id: string
  role: 'user' | 'assistant'
  content: string
  timestamp: string
}

export interface ConversationDetail {
  id: string
  userId?: string
  userName?: string
  platform: string
  messages: Message[]
  startedAt: string
  endedAt?: string
  status: string
}

// Pagination Types
export interface PaginationParams {
  page?: number
  pageSize?: number
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages?: number
}
