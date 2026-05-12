export interface Player {
  id: string
  userId: string
  name: string
  nickname: string | null
  phone: string | null
  dateOfBirth: string | null
  positionIds?: string[]
  isActive: boolean
  createdAt: string
}

export interface Position {
  id: string
  tenantId: string
  code: string
  name: string
  description: string | null
  isActive: boolean
  createdAt: string
}

export interface PlayerPositionsResponse {
  playerId: string
  positionIds: string[]
  updatedAt: string | null
}

export interface CreatePlayerRequest {
  userId: string
  name: string
  nickname?: string | null
  phone?: string | null
  dateOfBirth?: string | null
}

export interface UpdatePlayerRequest {
  name: string
  nickname?: string | null
  phone?: string | null
  dateOfBirth?: string | null
}

export interface UpdatePlayerPositionsRequest {
  positionIds: string[]
}