export interface CreateAssociationRequest {
  name: string
  slug: string
  logo: File
  street: string
  number: string
  neighborhood?: string
  city: string
  state: string
  zipCode: string
  associationLatitude: number
  associationLongitude: number
  adminEmail: string
  adminPassword: string
}

export interface AssociationResponse {
  id: string
  name: string
  slug: string
  provisioningStatus: string
  logoPath?: string | null
  street?: string | null
  number?: string | null
  neighborhood?: string | null
  city?: string | null
  state?: string | null
  zipCode?: string | null
  associationLatitude?: number | null
  associationLongitude?: number | null
}

export interface AssociationStatusRouteSearch {
  tenantId?: string
}
