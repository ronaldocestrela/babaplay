/** Rotas da API — espelha os endpoints definidos no AuthController e futuros controllers. */
export const API_ROUTES = {
  AUTH: {
    LOGIN: '/api/v1/auth/login',
    REFRESH_TOKEN: '/api/v1/auth/refresh-token',
    LOGOUT: '/api/v1/auth/logout',
    ME: '/api/v1/auth/me',
  },
  TENANT: {
    CREATE: '/api/v1/tenant',
    STATUS: (id: string) => `/api/v1/tenant/${id}/status`,
    SETTINGS: '/api/v1/tenant/settings',
    GAME_DAY_OPTIONS: '/api/v1/tenant/settings/game-day-options',
    GAME_DAY_OPTION_STATUS: (id: string) => `/api/v1/tenant/settings/game-day-options/${id}/status`,
  },
  ASSOCIATION_INVITE: {
    SEND: '/api/v1/association-invite',
    VALIDATE: '/api/v1/association-invite/validate',
    ACCEPT: '/api/v1/association-invite/accept',
    REGISTER_ACCEPT: '/api/v1/association-invite/register-accept',
  },
  PLAYER: {
    LIST: '/api/v1/player',
    BY_ID: (id: string) => `/api/v1/player/${id}`,
    UPDATE_POSITIONS: (id: string) => `/api/v1/player/${id}/positions`,
  },
  POSITION: {
    LIST: '/api/v1/position',
    BY_ID: (id: string) => `/api/v1/position/${id}`,
  },
  TEAM: {
    LIST: '/api/v1/team',
    BY_ID: (id: string) => `/api/v1/team/${id}`,
    PLAYERS: (id: string) => `/api/v1/team/${id}/players`,
  },
  GAMEDAY: {
    LIST: '/api/v1/gameday',
  },
  MATCH: {
    LIST: '/api/v1/match',
    BY_ID: (id: string) => `/api/v1/match/${id}`,
    STATUS: (id: string) => `/api/v1/match/${id}/status`,
  },
  CHECKIN: {
    LIST: '/api/v1/checkin',
    BY_ID: (id: string) => `/api/v1/checkin/${id}`,
    BY_GAMEDAY: (gameDayId: string) => `/api/v1/checkin/gameday/${gameDayId}`,
    BY_PLAYER: (playerId: string) => `/api/v1/checkin/player/${playerId}`,
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
