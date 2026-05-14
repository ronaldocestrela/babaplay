import { useMutation, useQuery } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { associationService } from '../services/associationService'
import type { CreateAssociationRequest } from '../types'

const TENANT_TERMINAL_STATUSES = new Set(['ready', 'failed', 'cancelled'])

export const ASSOCIATION_STATUS_QUERY_KEY = (tenantId: string) =>
  ['association-status', tenantId] as const

export function useCreateAssociation() {
  const { mutate: createAssociation, isPending, error, isError } = useMutation({
    mutationFn: (payload: CreateAssociationRequest) => associationService.createAssociation(payload),
  })

  return {
    createAssociation,
    isPending,
    isError,
    error,
    errorCode: getErrorCode(error),
  }
}

export function useAssociationStatus(tenantId?: string) {
  return useQuery({
    queryKey: ASSOCIATION_STATUS_QUERY_KEY(tenantId ?? 'unknown'),
    queryFn: async () => associationService.getAssociationStatus(tenantId ?? ''),
    enabled: Boolean(tenantId),
    retry: false,
    refetchInterval: (query) => {
      const status = query.state.data?.provisioningStatus?.toLowerCase()
      if (status && TENANT_TERMINAL_STATUSES.has(status)) {
        return false
      }

      return 3000
    },
  })
}
