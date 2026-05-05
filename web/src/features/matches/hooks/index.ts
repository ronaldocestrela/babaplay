import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { matchService } from '../services/matchService'
import type {
  ChangeMatchStatusRequest,
  CreateMatchRequest,
  UpdateMatchRequest,
} from '../types'

export const MATCHES_QUERY_KEY = ['matches'] as const
export const MATCH_DETAIL_QUERY_KEY = (id: string) => [...MATCHES_QUERY_KEY, id] as const
export const MATCH_GAMEDAYS_QUERY_KEY = [...MATCHES_QUERY_KEY, 'gamedays'] as const
export const MATCH_TEAMS_QUERY_KEY = [...MATCHES_QUERY_KEY, 'teams'] as const

export function useMatches() {
  return useQuery({
    queryKey: MATCHES_QUERY_KEY,
    queryFn: matchService.getMatches,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function useMatch(id?: string) {
  return useQuery({
    queryKey: MATCH_DETAIL_QUERY_KEY(id ?? 'unknown'),
    queryFn: async () => matchService.getMatchById(id ?? ''),
    enabled: Boolean(id),
    retry: false,
  })
}

export function useMatchGameDays() {
  return useQuery({
    queryKey: MATCH_GAMEDAYS_QUERY_KEY,
    queryFn: matchService.getGameDaysForMatches,
    staleTime: 5 * 60 * 1000,
    retry: false,
  })
}

export function useMatchTeams() {
  return useQuery({
    queryKey: MATCH_TEAMS_QUERY_KEY,
    queryFn: matchService.getTeamsForMatches,
    staleTime: 5 * 60 * 1000,
    retry: false,
  })
}

export function useCreateMatch() {
  const queryClient = useQueryClient()

  const { mutate: createMatch, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreateMatchRequest) => matchService.createMatch(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: MATCHES_QUERY_KEY })
    },
  })

  return {
    createMatch,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdateMatch() {
  const queryClient = useQueryClient()

  const { mutate: updateMatch, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateMatchRequest }) =>
      matchService.updateMatch(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: MATCHES_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: MATCH_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    updateMatch,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useChangeMatchStatus() {
  const queryClient = useQueryClient()

  const { mutate: changeMatchStatus, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: ChangeMatchStatusRequest }) =>
      matchService.changeStatus(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: MATCHES_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: MATCH_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    changeMatchStatus,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useDeleteMatch() {
  const queryClient = useQueryClient()

  const { mutate: deleteMatch, isPending, error, isError } = useMutation({
    mutationFn: (id: string) => matchService.deleteMatch(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: MATCHES_QUERY_KEY })
    },
  })

  return {
    deleteMatch,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
