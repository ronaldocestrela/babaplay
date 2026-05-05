import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { AssociationResponse, CreateAssociationRequest } from '../types'

export const associationService = {
  createAssociation: (payload: CreateAssociationRequest): Promise<AssociationResponse> => {
    const formData = new FormData()
    formData.append('Name', payload.name)
    formData.append('Slug', payload.slug)
    formData.append('Logo', payload.logo)
    formData.append('Street', payload.street)
    formData.append('Number', payload.number)

    if (payload.neighborhood) {
      formData.append('Neighborhood', payload.neighborhood)
    }

    formData.append('City', payload.city)
    formData.append('State', payload.state)
    formData.append('ZipCode', payload.zipCode)
    formData.append('AdminEmail', payload.adminEmail)
    formData.append('AdminPassword', payload.adminPassword)

    const testHeaders =
      import.meta.env.MODE === 'test'
        ? {
            'X-Association-Name': payload.name,
            'X-Association-Slug': payload.slug,
            'X-Association-Street': payload.street,
            'X-Association-Number': payload.number,
            'X-Association-Neighborhood': payload.neighborhood ?? '',
            'X-Association-City': payload.city,
            'X-Association-State': payload.state,
            'X-Association-ZipCode': payload.zipCode,
            'X-Association-AdminEmail': payload.adminEmail,
            'X-Association-AdminPassword': payload.adminPassword,
          }
        : undefined

    return apiClient
      .post<AssociationResponse>(API_ROUTES.TENANT.CREATE, formData, {
        headers: testHeaders,
      })
      .then((res) => res.data)
  },

  getAssociationStatus: (id: string): Promise<AssociationResponse> =>
    apiClient.get<AssociationResponse>(API_ROUTES.TENANT.STATUS(id)).then((res) => res.data),
}
