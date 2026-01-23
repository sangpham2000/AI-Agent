import apiClient from './client'

export interface HealthStatus {
  status: string
  timestamp: string
  version?: string
}

/**
 * Health check API
 */
export const healthApi = {
  /**
   * Check API health status
   */
  check: () => apiClient.get<HealthStatus>('/health'),
}

export default healthApi
