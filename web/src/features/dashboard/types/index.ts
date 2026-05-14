export interface PlayerSummary {
  id: string
  isActive: boolean
}

export interface TeamSummary {
  id: string
  isActive: boolean
}

export interface GameDaySummary {
  id: string
  scheduledAt: string
  status: string
}

export interface MatchSummary {
  id: string
  status: string
}

export interface CheckinSummary {
  id: string
  isActive: boolean
}

export interface RankingEntry {
  rank: number
  playerId: string
  scoreTotal: number
  attendanceCount: number
  goals: number
}

export interface TopScorerEntry {
  rank: number
  playerId: string
  goals: number
  scoreTotal: number
}

export interface AttendanceEntry {
  rank: number
  playerId: string
  attendanceCount: number
  scoreTotal: number
}

export interface CashFlowSummary {
  fromUtc: string
  toUtc: string
  totalIncome: number
  totalExpense: number
  balance: number
}

export interface DelinquencySummary {
  referenceUtc: string
  totalOpenAmount: number
}

export interface MonthlySummary {
  year: number
  month: number
  monthlyFeesAmount: number
  monthlyFeesPaidAmount: number
  monthlyFeesOpenAmount: number
  cashIncome: number
  cashExpense: number
  cashBalance: number
}

export interface DashboardOperationalKpis {
  activePlayers: number
  activeTeams: number
  upcomingGameDays: number
  liveMatches: number
  todayCheckins: number
}

export interface DashboardRankingWidget {
  available: boolean
  errorCode: string | null
  bestScore: number
  topScorers: TopScorerEntry[]
  attendanceLeaders: AttendanceEntry[]
}

export interface DashboardFinancialWidget {
  available: boolean
  errorCode: string | null
  cashBalance: number
  openAmount: number
  monthlyFeesPaidAmount: number
}

export interface DashboardOverview {
  operational: DashboardOperationalKpis
  ranking: DashboardRankingWidget
  financial: DashboardFinancialWidget
}
