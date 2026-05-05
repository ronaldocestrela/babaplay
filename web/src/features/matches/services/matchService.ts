import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type {
  ChangeMatchStatusRequest,
  CreateMatchRequest,
  Match,
  MatchGameDayOption,
  MatchTeamOption,
  UpdateMatchRequest,
} from '../types'

export const matchService = {
  getMatches: (): Promise<Match[]> =>
    apiClient.get<Match[]>(API_ROUTES.MATCH.LIST).then((res) => res.data),

  getMatchById: (id: string): Promise<Match> =>
    apiClient.get<Match>(API_ROUTES.MATCH.BY_ID(id)).then((res) => res.data),

  createMatch: (payload: CreateMatchRequest): Promise<Match> =>
    apiClient.post<Match>(API_ROUTES.MATCH.LIST, payload).then((res) => res.data),

  updateMatch: (id: string, payload: UpdateMatchRequest): Promise<Match> =>
    apiClient.put<Match>(API_ROUTES.MATCH.BY_ID(id), payload).then((res) => res.data),

  changeStatus: (id: string, payload: ChangeMatchStatusRequest): Promise<Match> =>
    apiClient.put<Match>(API_ROUTES.MATCH.STATUS(id), payload).then((res) => res.data),

  deleteMatch: async (id: string): Promise<void> => {
    await apiClient.delete(API_ROUTES.MATCH.BY_ID(id))
  },

  getGameDaysForMatches: (): Promise<MatchGameDayOption[]> =>
    apiClient.get<MatchGameDayOption[]>(API_ROUTES.GAMEDAY.LIST).then((res) => res.data),

  getTeamsForMatches: (): Promise<MatchTeamOption[]> =>
    apiClient.get<MatchTeamOption[]>(API_ROUTES.TEAM.LIST).then((res) => res.data),
}
