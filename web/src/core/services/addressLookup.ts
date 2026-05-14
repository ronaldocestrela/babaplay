export interface ZipCodeAddress {
  street: string
  neighborhood: string
  city: string
  state: string
}

export interface GeocodeLookupInput {
  zipCode?: string
  street?: string
  number?: string
  neighborhood?: string
  city?: string
  state?: string
}

function normalizeZipCode(value: string): string {
  return value.replace(/\D/g, '')
}

export async function lookupAddressByZipCode(zipCode: string): Promise<ZipCodeAddress | null> {
  const normalizedZipCode = normalizeZipCode(zipCode)
  if (normalizedZipCode.length !== 8) {
    return null
  }

  const response = await fetch(`https://viacep.com.br/ws/${normalizedZipCode}/json/`)
  if (!response.ok) {
    throw new Error('ZIP_LOOKUP_FAILED')
  }

  const data = (await response.json()) as {
    erro?: boolean
    logradouro?: string
    bairro?: string
    localidade?: string
    uf?: string
  }

  if (data.erro) {
    return null
  }

  return {
    street: data.logradouro?.trim() ?? '',
    neighborhood: data.bairro?.trim() ?? '',
    city: data.localidade?.trim() ?? '',
    state: data.uf?.trim() ?? '',
  }
}

async function fetchCoordinatesByQuery(query: string): Promise<{ latitude: number; longitude: number } | null> {
  const normalizedQuery = query.trim()
  if (!normalizedQuery) {
    return null
  }

  const params = new URLSearchParams({
    format: 'json',
    limit: '1',
    countrycodes: 'br',
    q: normalizedQuery,
  })

  const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`)
  if (!response.ok) {
    throw new Error('GEOCODING_FAILED')
  }

  const data = (await response.json()) as Array<{ lat: string; lon: string }>
  if (!Array.isArray(data) || data.length === 0) {
    return null
  }

  const latitude = Number(data[0]?.lat)
  const longitude = Number(data[0]?.lon)
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
    return null
  }

  return { latitude, longitude }
}

function getGeocodingCandidates(input: GeocodeLookupInput): string[] {
  const zipCode = input.zipCode?.replace(/\D/g, '') ?? ''
  const street = input.street?.trim() ?? ''
  const number = input.number?.trim() ?? ''
  const neighborhood = input.neighborhood?.trim() ?? ''
  const city = input.city?.trim() ?? ''
  const state = input.state?.trim() ?? ''

  const candidates = [
    zipCode ? `${zipCode}, Brasil` : '',
    [street, number, neighborhood, city, state, zipCode, 'Brasil'].filter(Boolean).join(', '),
    [street, neighborhood, city, state, 'Brasil'].filter(Boolean).join(', '),
    [city, state, zipCode, 'Brasil'].filter(Boolean).join(', '),
  ]

  return [...new Set(candidates.map((item) => item.trim()).filter(Boolean))]
}

export async function geocodeAddress(input: string | GeocodeLookupInput): Promise<{ latitude: number; longitude: number } | null> {
  if (typeof input === 'string') {
    return fetchCoordinatesByQuery(input)
  }

  const candidates = getGeocodingCandidates(input)
  for (const candidate of candidates) {
    const found = await fetchCoordinatesByQuery(candidate)
    if (found) {
      return found
    }
  }

  return null
}
