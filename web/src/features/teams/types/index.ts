export interface Team {
  id: string
  tenantId: string
  name: string
  maxPlayers: number
  isActive: boolean
  createdAt: string
  playerIds: string[]
}

export interface TeamPlayersResponse {
  teamId: string
  playerIds: string[]
  updatedAt: string | null
}

export interface TeamPlayerOption {
  id: string
  name: string
  isActive: boolean
}

export interface CreateTeamRequest {
  name: string
  maxPlayers: number
}

export interface UpdateTeamRequest {
  name: string
  maxPlayers: number
}

export interface UpdateTeamPlayersRequest {
  playerIds: string[]
}
