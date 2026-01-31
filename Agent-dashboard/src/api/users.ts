import apiClient from './client'
import type {
  User,
  CreateUser,
  UpdateUser,
  UserPermissions,
  Role,
  Permission,
  UserQuota,
} from './types'

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

  getByEmail: (email: string) => apiClient.get<User>(`/users/by-email/${email}`),

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

  assignRole(id: string, roleName: string) {
    return apiClient.post(`/users/${id}/roles`, JSON.stringify(roleName), {
      headers: {
        'Content-Type': 'application/json',
      },
    })
  },

  getPermissions(id: string) {
    return apiClient.get<UserPermissions>(`/users/${id}/permissions`)
  },

  getQuota(id: string) {
    return apiClient.get<UserQuota>(`/users/${id}/quota`)
  },
}

export const rolesApi = {
  getAll: () => apiClient.get<Role[]>('/roles'),
  getPermissions: () => apiClient.get<Permission[]>('/roles/permissions'),
  updatePermissions: (roleId: string, permissionIds: string[]) =>
    apiClient.put(`/roles/${roleId}/permissions`, permissionIds),
}

export default usersApi
