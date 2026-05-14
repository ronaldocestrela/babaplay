export interface TenantGameDayOptionResponse {
  id: string
  tenantId: string
  dayOfWeek: number
  localStartTime: string
  isActive: boolean
  createdAt: string
  updatedAt?: string | null
}

export interface CreateTenantGameDayOptionRequest {
  dayOfWeek: number
  localStartTime: string
}

export interface ChangeTenantGameDayOptionStatusRequest {
  id: string
  isActive: boolean
}
