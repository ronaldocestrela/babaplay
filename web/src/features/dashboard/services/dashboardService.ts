import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type {
  AttendanceEntry,
  CashFlowSummary,
  CheckinSummary,
  DelinquencySummary,
  GameDaySummary,
  MatchSummary,
  MonthlySummary,
  PlayerSummary,
  RankingEntry,
  TeamSummary,
  TopScorerEntry,
} from '../types'

interface RankingParams {
  page?: number
  pageSize?: number
  fromUtc?: string
  toUtc?: string
}

export const dashboardService = {
  getPlayers: (): Promise<PlayerSummary[]> =>
    apiClient.get<PlayerSummary[]>(API_ROUTES.PLAYER.LIST).then((res) => res.data),

  getTeams: (): Promise<TeamSummary[]> =>
    apiClient.get<TeamSummary[]>(API_ROUTES.TEAM.LIST).then((res) => res.data),

  getGameDays: (): Promise<GameDaySummary[]> =>
    apiClient.get<GameDaySummary[]>(API_ROUTES.GAMEDAY.LIST).then((res) => res.data),

  getMatches: (): Promise<MatchSummary[]> =>
    apiClient.get<MatchSummary[]>(API_ROUTES.MATCH.LIST).then((res) => res.data),

  getCheckinsByGameDay: (gameDayId: string): Promise<CheckinSummary[]> =>
    apiClient
      .get<CheckinSummary[]>(API_ROUTES.CHECKIN.BY_GAMEDAY(gameDayId))
      .then((res) => res.data),

  getRanking: (params: RankingParams): Promise<RankingEntry[]> =>
    apiClient
      .get<RankingEntry[]>(API_ROUTES.RANKING.LIST, { params })
      .then((res) => res.data),

  getTopScorers: (params: RankingParams): Promise<TopScorerEntry[]> =>
    apiClient
      .get<TopScorerEntry[]>(API_ROUTES.RANKING.TOP_SCORERS, { params })
      .then((res) => res.data),

  getAttendanceRanking: (params: RankingParams): Promise<AttendanceEntry[]> =>
    apiClient
      .get<AttendanceEntry[]>(API_ROUTES.RANKING.ATTENDANCE, { params })
      .then((res) => res.data),

  getCashFlow: (fromUtc: string, toUtc: string): Promise<CashFlowSummary> =>
    apiClient
      .get<CashFlowSummary>(API_ROUTES.FINANCIAL.CASH_FLOW, { params: { fromUtc, toUtc } })
      .then((res) => res.data),

  getDelinquency: (referenceUtc: string): Promise<DelinquencySummary> =>
    apiClient
      .get<DelinquencySummary>(API_ROUTES.FINANCIAL.DELINQUENCY, {
        params: { referenceUtc },
      })
      .then((res) => res.data),

  getMonthlySummary: (year: number, month: number): Promise<MonthlySummary> =>
    apiClient
      .get<MonthlySummary>(API_ROUTES.FINANCIAL.MONTHLY_SUMMARY, {
        params: { year, month },
      })
      .then((res) => res.data),
}
