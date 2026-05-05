/** Rotas da API — espelha os endpoints definidos no AuthController e futuros controllers. */
export const API_ROUTES = {
  AUTH: {
    LOGIN: '/api/v1/auth/login',
    REFRESH_TOKEN: '/api/v1/auth/refresh-token',
    LOGOUT: '/api/v1/auth/logout',
    ME: '/api/v1/auth/me',
  },
  PLAYER: {
    LIST: '/api/v1/player',
  },
  TEAM: {
    LIST: '/api/v1/team',
  },
  GAMEDAY: {
    LIST: '/api/v1/gameday',
  },
  MATCH: {
    LIST: '/api/v1/match',
  },
  CHECKIN: {
    BY_GAMEDAY: (gameDayId: string) => `/api/v1/checkin/gameday/${gameDayId}`,
  },
  RANKING: {
    LIST: '/api/v1/ranking',
    TOP_SCORERS: '/api/v1/ranking/top-scorers',
    ATTENDANCE: '/api/v1/ranking/attendance',
  },
  FINANCIAL: {
    CASH_FLOW: '/api/v1/financial/cash-flow',
    DELINQUENCY: '/api/v1/financial/delinquency',
    MONTHLY_SUMMARY: '/api/v1/financial/monthly-summary',
  },
} as const
