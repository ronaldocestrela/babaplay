import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { positionService } from '../services/positionService'
import type { CreatePositionRequest, UpdatePositionRequest } from '../types'

export const POSITIONS_QUERY_KEY = ['positions'] as const

export function usePositions() {
  return useQuery({
    queryKey: POSITIONS_QUERY_KEY,
    queryFn: positionService.getPositions,
    staleTime: 10 * 60 * 1000,
    retry: false,
  })
}

export function useCreatePosition() {
  const queryClient = useQueryClient()

  const { mutate: createPosition, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreatePositionRequest) => positionService.createPosition(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: POSITIONS_QUERY_KEY })
    },
  })

  return {
    createPosition,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useUpdatePosition() {
  const queryClient = useQueryClient()

  const { mutate: updatePosition, isPending, error, isError } = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdatePositionRequest }) =>
      positionService.updatePosition(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: POSITIONS_QUERY_KEY })
    },
  })

  return {
    updatePosition,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useDeletePosition() {
  const queryClient = useQueryClient()

  const { mutate: deletePosition, isPending, error, isError } = useMutation({
    mutationFn: (id: string) => positionService.deletePosition(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: POSITIONS_QUERY_KEY })
    },
  })

  return {
    deletePosition,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}