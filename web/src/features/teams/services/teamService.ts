import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type {
  CreateTeamRequest,
  Team,
  TeamPlayerOption,
  TeamPlayersResponse,
  UpdateTeamPlayersRequest,
  UpdateTeamRequest,
} from '../types'

export const teamService = {
  getTeams: (): Promise<Team[]> =>
    apiClient.get<Team[]>(API_ROUTES.TEAM.LIST).then((res) => res.data),

  getTeamById: (id: string): Promise<Team> =>
    apiClient.get<Team>(API_ROUTES.TEAM.BY_ID(id)).then((res) => res.data),

  createTeam: (payload: CreateTeamRequest): Promise<Team> =>
    apiClient.post<Team>(API_ROUTES.TEAM.LIST, payload).then((res) => res.data),

  updateTeam: (id: string, payload: UpdateTeamRequest): Promise<Team> =>
    apiClient.put<Team>(API_ROUTES.TEAM.BY_ID(id), payload).then((res) => res.data),

  deleteTeam: async (id: string): Promise<void> => {
    await apiClient.delete(API_ROUTES.TEAM.BY_ID(id))
  },

  updateTeamPlayers: (
    id: string,
    payload: UpdateTeamPlayersRequest,
  ): Promise<TeamPlayersResponse> =>
    apiClient.put<TeamPlayersResponse>(API_ROUTES.TEAM.PLAYERS(id), payload).then((res) => res.data),

  getPlayersForRoster: (): Promise<TeamPlayerOption[]> =>
    apiClient.get<TeamPlayerOption[]>(API_ROUTES.PLAYER.LIST).then((res) => res.data),
}
