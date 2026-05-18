import { useMutation } from '@tanstack/react-query'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { associationService } from '../services/associationService'
import type { CreateAssociationRequest } from '../types'

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
