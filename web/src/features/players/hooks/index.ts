import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { playerService } from '../services/playerService'
import type {
  CreatePlayerRequest,
  UpdatePlayerPositionsRequest,
  UpdatePlayerRequest,
} from '../types'

export const PLAYERS_QUERY_KEY = ['players'] as const
export const POSITIONS_QUERY_KEY = ['positions'] as const
export const PLAYER_DETAIL_QUERY_KEY = (id: string) => [...PLAYERS_QUERY_KEY, id] as const

export function usePlayers() {
  return useQuery({
    queryKey: PLAYERS_QUERY_KEY,
    queryFn: playerService.getPlayers,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function usePlayer(id?: string) {
  return useQuery({
    queryKey: PLAYER_DETAIL_QUERY_KEY(id ?? 'unknown'),
    queryFn: async () => playerService.getPlayerById(id ?? ''),
    enabled: Boolean(id),
    retry: false,
  })
}

export function usePositions() {
  return useQuery({
    queryKey: POSITIONS_QUERY_KEY,
    queryFn: playerService.getPositions,
    staleTime: 10 * 60 * 1000,
    retry: false,
  })
}

export function useCreatePlayer() {
  const queryClient = useQueryClient()

  const { mutate: createPlayer, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreatePlayerRequest) => playerService.createPlayer(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: PLAYERS_QUERY_KEY })
    },
  })

  return {
    createPlayer,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdatePlayer() {
  const queryClient = useQueryClient()

  const { mutate: updatePlayer, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdatePlayerRequest }) =>
      playerService.updatePlayer(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: PLAYERS_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: PLAYER_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    updatePlayer,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useDeletePlayer() {
  const queryClient = useQueryClient()

  const { mutate: deletePlayer, isPending, error, isError } = useMutation({
    mutationFn: (id: string) => playerService.deletePlayer(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: PLAYERS_QUERY_KEY })
    },
  })

  return {
    deletePlayer,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdatePlayerPositions() {
  const queryClient = useQueryClient()

  const { mutate: updatePlayerPositions, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdatePlayerPositionsRequest }) =>
      playerService.updatePlayerPositions(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: PLAYERS_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: PLAYER_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    updatePlayerPositions,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}