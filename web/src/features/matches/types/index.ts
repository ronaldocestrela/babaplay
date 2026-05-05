export type MatchStatus = 'Pending' | 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled'

export interface Match {
  id: string
  tenantId: string
  gameDayId: string
  homeTeamId: string
  awayTeamId: string
  description: string | null
  status: MatchStatus
  isActive: boolean
  createdAt: string
}

export interface MatchGameDayOption {
  id: string
  name?: string | null
  scheduledAt: string
  status: string
}

export interface MatchTeamOption {
  id: string
  name: string
  isActive: boolean
}

export interface CreateMatchRequest {
  gameDayId: string
  homeTeamId: string
  awayTeamId: string
  description?: string | null
}

export interface UpdateMatchRequest {
  gameDayId: string
  homeTeamId: string
  awayTeamId: string
  description?: string | null
}

export interface ChangeMatchStatusRequest {
  status: MatchStatus
}
