import { apiClient } from '@/core/api/client'
import { API_ROUTES } from '@/core/constants/apiRoutes'
import type { TenantSettingsResponse, UpdateTenantSettingsRequest } from '../types'

export const tenantSettingsService = {
  getSettings: (): Promise<TenantSettingsResponse> =>
    apiClient
      .get<TenantSettingsResponse>(API_ROUTES.TENANT.SETTINGS)
      .then((res) => res.data),

  updateSettings: (payload: UpdateTenantSettingsRequest): Promise<TenantSettingsResponse> => {
    const formData = new FormData()
    formData.append('Name', payload.name)
    formData.append('PlayersPerTeam', String(payload.playersPerTeam))
    formData.append('Street', payload.street)
    formData.append('Number', payload.number)
    if (payload.neighborhood) formData.append('Neighborhood', payload.neighborhood)
    formData.append('City', payload.city)
    formData.append('State', payload.state)
    formData.append('ZipCode', payload.zipCode)
    formData.append('AssociationLatitude', String(payload.associationLatitude))
    formData.append('AssociationLongitude', String(payload.associationLongitude))
    if (payload.logo) formData.append('Logo', payload.logo)

    const testHeaders =
      import.meta.env.MODE === 'test'
        ? {
            'X-Tenant-Name': payload.name,
            'X-Tenant-PlayersPerTeam': String(payload.playersPerTeam),
            'X-Tenant-Street': payload.street,
            'X-Tenant-Number': payload.number,
            'X-Tenant-Neighborhood': payload.neighborhood ?? '',
            'X-Tenant-City': payload.city,
            'X-Tenant-State': payload.state,
            'X-Tenant-ZipCode': payload.zipCode,
            'X-Tenant-Latitude': String(payload.associationLatitude),
            'X-Tenant-Longitude': String(payload.associationLongitude),
          }
        : undefined

    return apiClient
      .put<TenantSettingsResponse>(API_ROUTES.TENANT.SETTINGS, formData, {
        headers: testHeaders,
      })
      .then((res) => res.data)
  },
}
