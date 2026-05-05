import type { CreateTenantRequest, TenantResponse } from '@/features/auth/types'

export type CreateAssociationRequest = CreateTenantRequest
export type AssociationResponse = TenantResponse

export interface AssociationStatusRouteSearch {
  tenantId?: string
}
