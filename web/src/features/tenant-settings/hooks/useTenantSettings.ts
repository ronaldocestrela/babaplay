import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { tenantSettingsService } from '../services/tenantSettingsService'
import type { UpdateTenantSettingsRequest } from '../types'

export const TENANT_SETTINGS_QUERY_KEY = ['tenant-settings'] as const

export function useTenantSettings() {
  return useQuery({
    queryKey: TENANT_SETTINGS_QUERY_KEY,
    queryFn: tenantSettingsService.getSettings,
    retry: false,
  })
}

export function useUpdateTenantSettings() {
  const queryClient = useQueryClient()
  const { mutate: updateSettings, isPending, error, isError } = useMutation({
    mutationFn: (payload: UpdateTenantSettingsRequest) => tenantSettingsService.updateSettings(payload),
    onSuccess: (data) => {
      queryClient.setQueryData(TENANT_SETTINGS_QUERY_KEY, data)
    },
  })

  return {
    updateSettings,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}
