import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { AssociationResponse, CreateAssociationRequest } from '../types'

export const associationService = {
  createAssociation: (payload: CreateAssociationRequest): Promise<AssociationResponse> =>
    apiClient.post<AssociationResponse>(API_ROUTES.TENANT.CREATE, payload).then((res) => res.data),

  getAssociationStatus: (id: string): Promise<AssociationResponse> =>
    apiClient.get<AssociationResponse>(API_ROUTES.TENANT.STATUS(id)).then((res) => res.data),
}
