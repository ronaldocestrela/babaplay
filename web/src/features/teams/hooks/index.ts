import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { teamService } from '../services/teamService'
import type {
  CreateTeamRequest,
  UpdateTeamPlayersRequest,
  UpdateTeamRequest,
} from '../types'

export const TEAMS_QUERY_KEY = ['teams'] as const
export const TEAM_DETAIL_QUERY_KEY = (id: string) => [...TEAMS_QUERY_KEY, id] as const
export const TEAM_PLAYERS_QUERY_KEY = [...TEAMS_QUERY_KEY, 'players'] as const

export function useTeams() {
  return useQuery({
    queryKey: TEAMS_QUERY_KEY,
    queryFn: teamService.getTeams,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function useTeam(id?: string) {
  return useQuery({
    queryKey: TEAM_DETAIL_QUERY_KEY(id ?? 'unknown'),
    queryFn: async () => teamService.getTeamById(id ?? ''),
    enabled: Boolean(id),
    retry: false,
  })
}

export function useTeamPlayers() {
  return useQuery({
    queryKey: TEAM_PLAYERS_QUERY_KEY,
    queryFn: teamService.getPlayersForRoster,
    staleTime: 2 * 60 * 1000,
    retry: false,
  })
}

export function useCreateTeam() {
  const queryClient = useQueryClient()

  const { mutate: createTeam, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreateTeamRequest) => teamService.createTeam(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: TEAMS_QUERY_KEY })
    },
  })

  return {
    createTeam,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdateTeam() {
  const queryClient = useQueryClient()

  const { mutate: updateTeam, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTeamRequest }) =>
      teamService.updateTeam(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: TEAMS_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: TEAM_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    updateTeam,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useDeleteTeam() {
  const queryClient = useQueryClient()

  const { mutate: deleteTeam, isPending, error, isError } = useMutation({
    mutationFn: (id: string) => teamService.deleteTeam(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: TEAMS_QUERY_KEY })
    },
  })

  return {
    deleteTeam,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdateTeamPlayers() {
  const queryClient = useQueryClient()

  const { mutate: updateTeamPlayers, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateTeamPlayersRequest }) =>
      teamService.updateTeamPlayers(id, payload),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: TEAMS_QUERY_KEY }),
        queryClient.invalidateQueries({ queryKey: TEAM_DETAIL_QUERY_KEY(variables.id) }),
      ])
    },
  })

  return {
    updateTeamPlayers,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
