import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { tenantGameDayOptionsService } from '../services/tenantGameDayOptionsService'
import type {
  ChangeTenantGameDayOptionStatusRequest,
  CreateTenantGameDayOptionRequest,
} from '../types/gameDayOptions'

export const TENANT_GAMEDAY_OPTIONS_QUERY_KEY = ['tenant-gameday-options'] as const

export function useTenantGameDayOptions() {
  return useQuery({
    queryKey: TENANT_GAMEDAY_OPTIONS_QUERY_KEY,
    queryFn: tenantGameDayOptionsService.getOptions,
    retry: false,
  })
}

export function useCreateTenantGameDayOption() {
  const queryClient = useQueryClient()
  const { mutate: createOption, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreateTenantGameDayOptionRequest) => tenantGameDayOptionsService.createOption(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: TENANT_GAMEDAY_OPTIONS_QUERY_KEY })
    },
  })

  return {
    createOption,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useChangeTenantGameDayOptionStatus() {
  const queryClient = useQueryClient()
  const { mutate: changeStatus, isPending, error, isError } = useMutation({
    mutationFn: ({ id, isActive }: ChangeTenantGameDayOptionStatusRequest) =>
      tenantGameDayOptionsService.changeStatus({ id, isActive }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: TENANT_GAMEDAY_OPTIONS_QUERY_KEY })
    },
  })

  return {
    changeStatus,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
