import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { AssociationLocationMap } from '../AssociationLocationMap'

let clickHandler: ((event: { latlng: { lat: number; lng: number } }) => void) | undefined

vi.mock('react-leaflet', () => ({
  MapContainer: ({ children }: { children: ReactNode }) => <div data-testid="leaflet-map">{children}</div>,
  TileLayer: () => <div data-testid="leaflet-tile" />,
  CircleMarker: () => <div data-testid="leaflet-marker" />,
  useMapEvents: (events: { click?: (event: { latlng: { lat: number; lng: number } }) => void }) => {
    clickHandler = events.click
    return null
  },
}))

describe('AssociationLocationMap', () => {
  it('deve renderizar marcador quando coordenadas são válidas', () => {
    render(
      <AssociationLocationMap
        latitude="-23.5505"
        longitude="-46.6333"
        onCoordinateChange={() => {}}
      />,
    )

    expect(screen.getByTestId('association-location-map')).toBeInTheDocument()
    expect(screen.getByTestId('leaflet-map')).toBeInTheDocument()
    expect(screen.getByTestId('leaflet-marker')).toBeInTheDocument()
  })

  it('deve propagar coordenadas ao clicar no mapa', () => {
    const onCoordinateChange = vi.fn()

    render(
      <AssociationLocationMap
        latitude=""
        longitude=""
        onCoordinateChange={onCoordinateChange}
      />,
    )

    clickHandler?.({ latlng: { lat: -23.56789, lng: -46.67891 } })

    expect(onCoordinateChange).toHaveBeenCalledWith('-23.567890', '-46.678910')
  })

  it('não deve renderizar marcador quando coordenadas não são numéricas', () => {
    render(
      <AssociationLocationMap
        latitude="abc"
        longitude="def"
        onCoordinateChange={() => {}}
      />,
    )

    expect(screen.queryByTestId('leaflet-marker')).not.toBeInTheDocument()
  })

  it('não deve renderizar marcador quando coordenadas estão fora do intervalo válido', () => {
    render(
      <AssociationLocationMap
        latitude="91"
        longitude="181"
        onCoordinateChange={() => {}}
      />,
    )

    expect(screen.queryByTestId('leaflet-marker')).not.toBeInTheDocument()
  })
})
