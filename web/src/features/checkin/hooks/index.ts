import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { checkinService } from '../services/checkinService'
import type { CancelCheckinRequest, CreateCheckinRequest } from '../types'

export const CHECKINS_QUERY_KEY = ['checkins'] as const
export const CHECKIN_PLAYERS_QUERY_KEY = [...CHECKINS_QUERY_KEY, 'players'] as const
export const CHECKIN_GAMEDAYS_QUERY_KEY = [...CHECKINS_QUERY_KEY, 'gamedays'] as const
export const CHECKINS_BY_GAMEDAY_QUERY_KEY = (gameDayId: string) =>
  [...CHECKINS_QUERY_KEY, 'gameday', gameDayId] as const
export const CHECKINS_BY_PLAYER_QUERY_KEY = (playerId: string) =>
  [...CHECKINS_QUERY_KEY, 'player', playerId] as const

export function useCheckinPlayers() {
  return useQuery({
    queryKey: CHECKIN_PLAYERS_QUERY_KEY,
    queryFn: checkinService.getPlayersForCheckin,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function useCheckinGameDays() {
  return useQuery({
    queryKey: CHECKIN_GAMEDAYS_QUERY_KEY,
    queryFn: checkinService.getGameDaysForCheckin,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function useCheckinsByGameDay(gameDayId?: string) {
  return useQuery({
    queryKey: CHECKINS_BY_GAMEDAY_QUERY_KEY(gameDayId ?? 'unknown'),
    queryFn: async () => checkinService.getCheckinsByGameDay(gameDayId ?? ''),
    enabled: Boolean(gameDayId),
    staleTime: 60 * 1000,
    retry: false,
  })
}

export function useCheckinsByPlayer(playerId?: string) {
  return useQuery({
    queryKey: CHECKINS_BY_PLAYER_QUERY_KEY(playerId ?? 'unknown'),
    queryFn: async () => checkinService.getCheckinsByPlayer(playerId ?? ''),
    enabled: Boolean(playerId),
    staleTime: 60 * 1000,
    retry: false,
  })
}

export function useCreateCheckin() {
  const queryClient = useQueryClient()

  const { mutate: createCheckin, isPending, isError, error } = useMutation({
    mutationFn: (payload: CreateCheckinRequest) => checkinService.createCheckin(payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: CHECKINS_QUERY_KEY }),
        queryClient.invalidateQueries({
          queryKey: CHECKINS_BY_GAMEDAY_QUERY_KEY(variables.gameDayId),
        }),
        queryClient.invalidateQueries({
          queryKey: CHECKINS_BY_PLAYER_QUERY_KEY(variables.playerId),
        }),
      ])
    },
  })

  return {
    createCheckin,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useCancelCheckin() {
  const queryClient = useQueryClient()

  const { mutate: cancelCheckin, isPending, isError, error } = useMutation({
    mutationFn: ({ id }: CancelCheckinRequest) => checkinService.cancelCheckin(id),
    onSuccess: async (_, variables) => {
      const invalidations: Promise<unknown>[] = [
        queryClient.invalidateQueries({ queryKey: CHECKINS_QUERY_KEY }),
      ]

      if (variables.gameDayId) {
        invalidations.push(
          queryClient.invalidateQueries({
            queryKey: CHECKINS_BY_GAMEDAY_QUERY_KEY(variables.gameDayId),
          }),
        )
      }

      if (variables.playerId) {
        invalidations.push(
          queryClient.invalidateQueries({
            queryKey: CHECKINS_BY_PLAYER_QUERY_KEY(variables.playerId),
          }),
        )
      }

      await Promise.all(invalidations)
    },
  })

  return {
    cancelCheckin,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
