import apiClient from './client'
import type { User, CreateUser, UpdateUser } from './types'

/**
 * Users API endpoints
 */
export const usersApi = {
  /**
   * Get all users
   */
  getAll: () => apiClient.get<User[]>('/users'),

  /**
   * Get user by ID
   */
  getById: (id: string) => apiClient.get<User>(`/users/${id}`),

  /**
   * Create a new user
   */
  create: (data: CreateUser) => apiClient.post<User>('/users', data),

  /**
   * Update an existing user
   */
  update: (id: string, data: UpdateUser) => apiClient.put(`/users/${id}`, data),

  /**
   * Delete a user
   */
  delete: (id: string) => apiClient.delete(`/users/${id}`),
}

export default usersApi
