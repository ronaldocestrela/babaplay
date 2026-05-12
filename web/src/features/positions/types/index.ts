export interface Position {
  id: string
  tenantId: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
}

export interface CreatePositionRequest {
  code: string
  name: string
  description?: string | null
}

export interface UpdatePositionRequest {
  code: string
  name: string
  description?: string | null
}