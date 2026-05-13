export interface TenantSettingsResponse {
  id: string
  name: string
  slug: string
  provisioningStatus: string
  playersPerTeam: number
  logoPath?: string | null
  street?: string | null
  number?: string | null
  neighborhood?: string | null
  city?: string | null
  state?: string | null
  zipCode?: string | null
}

export interface UpdateTenantSettingsRequest {
  name: string
  playersPerTeam: number
  logo?: File
  street: string
  number: string
  neighborhood?: string
  city: string
  state: string
  zipCode: string
}
