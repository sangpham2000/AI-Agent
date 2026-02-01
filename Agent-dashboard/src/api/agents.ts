import apiClient from './client'

export interface Agent {
  id: string
  name: string
  description?: string
  flowiseChatflowId: string
  systemPrompt?: string
  flowiseConfig?: string
  isActive: boolean
  isDefault: boolean
}

export interface CreateAgent {
  name: string
  description?: string
  flowiseChatflowId: string
  systemPrompt?: string
  flowiseConfig?: string
  isActive: boolean
  isDefault: boolean
}

export interface UpdateAgent extends CreateAgent {
  id: string
}

export const agentsApi = {
  getAll: () => apiClient.get<Agent[]>('/agents'),
  getById: (id: string) => apiClient.get<Agent>(`/agents/${id}`),
  create: (data: CreateAgent) => apiClient.post<string>('/agents', data),
  update: (id: string, data: UpdateAgent) => apiClient.put<string>(`/agents/${id}`, data),
  delete: (id: string) => apiClient.delete(`/agents/${id}`),
}

export default agentsApi
