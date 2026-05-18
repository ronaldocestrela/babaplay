import { useQuery } from '@tanstack/react-query'
import type { AxiosError } from 'axios'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { useAuthStore } from '@/features/auth/store/authStore'
import { dashboardService } from '../services/dashboardService'
import type { DashboardOverview } from '../types'

export const DASHBOARD_OVERVIEW_QUERY_KEY = ['dashboard', 'overview'] as const

export interface DashboardPeriodFilter {
  fromUtc: string
  toUtc: string
}

function toIsoUtc(date: Date): string {
  return date.toISOString()
}

function isSameUtcDate(dateA: Date, dateB: Date): boolean {
  return (
    dateA.getUTCFullYear() === dateB.getUTCFullYear() &&
    dateA.getUTCMonth() === dateB.getUTCMonth() &&
    dateA.getUTCDate() === dateB.getUTCDate()
  )
}

function isForbidden(error: unknown): boolean {
  const axiosError = error as AxiosError
  return axiosError?.response?.status === 403
}

export function useDashboardData(period?: DashboardPeriodFilter) {
  const currentTenant = useAuthStore((state) => state.currentTenant)

  return useQuery({
    queryKey: [
      ...DASHBOARD_OVERVIEW_QUERY_KEY,
      currentTenant?.slug ?? null,
      period?.fromUtc ?? null,
      period?.toUtc ?? null,
    ],
    queryFn: async (): Promise<DashboardOverview> => {
      const now = new Date()
      const fromUtc = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1))
      const toUtc = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() + 1, 0, 23, 59, 59, 999))
      const selectedFromUtc = period?.fromUtc ?? toIsoUtc(fromUtc)
      const selectedToUtc = period?.toUtc ?? toIsoUtc(toUtc)

      const [playersResult, teamsResult, gameDaysResult, matchesResult] = await Promise.allSettled([
        dashboardService.getPlayers(),
        dashboardService.getTeams(),
        dashboardService.getGameDays(),
        dashboardService.getMatches(),
      ])

      const players = playersResult.status === 'fulfilled' ? playersResult.value : []
      const teams = teamsResult.status === 'fulfilled' ? teamsResult.value : []
      const gameDays = gameDaysResult.status === 'fulfilled' ? gameDaysResult.value : []
      const matches = matchesResult.status === 'fulfilled' ? matchesResult.value : []

      const todayGameDayIds = gameDays
        .filter((gameDay) => isSameUtcDate(new Date(gameDay.scheduledAt), now))
        .map((gameDay) => gameDay.id)

      const todayCheckinResults = await Promise.allSettled(
        todayGameDayIds.map((gameDayId) => dashboardService.getCheckinsByGameDay(gameDayId)),
      )

      const todayCheckins = todayCheckinResults.reduce(
        (acc, current) =>
          acc +
          (current.status === 'fulfilled'
            ? current.value.filter((checkin) => checkin.isActive).length
            : 0),
        0,
      )

      const rankingParams = {
        page: 1,
        pageSize: 5,
        fromUtc: selectedFromUtc,
        toUtc: selectedToUtc,
      }

      let rankingAvailable = true
      let rankingErrorCode: string | null = null
      let rankingBestScore = 0
      let rankingTopScorers = [] as Awaited<
        ReturnType<typeof dashboardService.getTopScorers>
      >
      let rankingAttendance = [] as Awaited<
        ReturnType<typeof dashboardService.getAttendanceRanking>
      >

      try {
        const [ranking, topScorers, attendance] = await Promise.all([
          dashboardService.getRanking(rankingParams),
          dashboardService.getTopScorers(rankingParams),
          dashboardService.getAttendanceRanking(rankingParams),
        ])

        rankingBestScore = ranking[0]?.scoreTotal ?? 0
        rankingTopScorers = topScorers
        rankingAttendance = attendance
      } catch (error) {
        if (isForbidden(error)) {
          rankingAvailable = false
          rankingErrorCode = 'FORBIDDEN'
        } else {
          rankingAvailable = false
          rankingErrorCode = getErrorCode(error) ?? 'RANKING_UNAVAILABLE'
        }
      }

      let financialAvailable = true
      let financialErrorCode: string | null = null
      let cashBalance = 0
      let openAmount = 0
      let monthlyFeesPaidAmount = 0

      try {
        const [cashFlow, delinquency, monthlySummary] = await Promise.all([
          dashboardService.getCashFlow(selectedFromUtc, selectedToUtc),
          dashboardService.getDelinquency(toIsoUtc(now)),
          dashboardService.getMonthlySummary(now.getUTCFullYear(), now.getUTCMonth() + 1),
        ])

        cashBalance = cashFlow.balance
        openAmount = delinquency.totalOpenAmount
        monthlyFeesPaidAmount = monthlySummary.monthlyFeesPaidAmount
      } catch (error) {
        if (isForbidden(error)) {
          financialAvailable = false
          financialErrorCode = 'FORBIDDEN'
        } else {
          financialAvailable = false
          financialErrorCode = getErrorCode(error) ?? 'FINANCIAL_UNAVAILABLE'
        }
      }

      return {
        operational: {
          activePlayers: players.filter((player) => player.isActive).length,
          activeTeams: teams.filter((team) => team.isActive).length,
          upcomingGameDays: gameDays.filter((gameDay) => new Date(gameDay.scheduledAt) >= now).length,
          liveMatches: matches.filter((match) => match.status === 'InProgress').length,
          todayCheckins,
        },
        ranking: {
          available: rankingAvailable,
          errorCode: rankingErrorCode,
          bestScore: rankingBestScore,
          topScorers: rankingTopScorers,
          attendanceLeaders: rankingAttendance,
        },
        financial: {
          available: financialAvailable,
          errorCode: financialErrorCode,
          cashBalance,
          openAmount,
          monthlyFeesPaidAmount,
        },
      }
    },
    enabled: currentTenant !== null,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}
