import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'

export interface AssociationInviteValidationResponse {
  tenantId: string
  tenantName: string
  tenantSlug: string
  email: string
  expiresAtUtc: string
  requiresRegistration: boolean
}

export interface AssociationInviteAcceptResponse {
  tenantId: string
  tenantName: string
  tenantSlug: string
  userId: string
  email: string
  alreadyMember: boolean
}

export interface AssociationInviteSendResponse {
  invitationId: string
  tenantId: string
  tenantSlug: string
  email: string
  expiresAtUtc: string
}

export const invitationService = {
  async send(email: string) {
    const { data } = await apiClient.post<AssociationInviteSendResponse>(
      API_ROUTES.ASSOCIATION_INVITE.SEND,
      { email },
    )
    return data
  },

  async validate(token: string) {
    const { data } = await apiClient.get<AssociationInviteValidationResponse>(
      API_ROUTES.ASSOCIATION_INVITE.VALIDATE,
      { params: { token } },
    )
    return data
  },

  async accept(token: string) {
    const { data } = await apiClient.post<AssociationInviteAcceptResponse>(
      API_ROUTES.ASSOCIATION_INVITE.ACCEPT,
      { token },
    )
    return data
  },

  async registerAndAccept(token: string, email: string, password: string) {
    const { data } = await apiClient.post<AssociationInviteAcceptResponse>(
      API_ROUTES.ASSOCIATION_INVITE.REGISTER_ACCEPT,
      { token, email, password },
    )
    return data
  },
}
