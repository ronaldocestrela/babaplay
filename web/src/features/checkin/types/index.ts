export interface Checkin {
  id: string
  tenantId: string
  playerId: string
  gameDayId: string
  checkedInAtUtc: string
  latitude: number
  longitude: number
  distanceFromAssociationMeters: number
  isActive: boolean
  createdAt: string
  cancelledAtUtc: string | null
}

export interface CreateCheckinRequest {
  playerId: string
  gameDayId: string
  checkedInAtUtc: string
  latitude: number
  longitude: number
}

export interface CheckinPlayerOption {
  id: string
  name: string
  isActive: boolean
}

export interface CheckinGameDayOption {
  id: string
  scheduledAt: string
  status: string
}

export interface CancelCheckinRequest {
  id: string
  gameDayId?: string
  playerId?: string
}
