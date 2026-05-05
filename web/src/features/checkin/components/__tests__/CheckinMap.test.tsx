import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { CheckinMap } from '../CheckinMap'

vi.mock('react-leaflet', () => ({
  MapContainer: ({ children }: { children: ReactNode }) => (
    <div data-testid="leaflet-map">{children}</div>
  ),
  TileLayer: () => <div data-testid="leaflet-tile" />,
  CircleMarker: ({ children }: { children?: ReactNode }) => (
    <div data-testid="leaflet-marker">{children}</div>
  ),
  Popup: ({ children }: { children?: ReactNode }) => <div>{children}</div>,
}))

describe('CheckinMap', () => {
  it('deve renderizar mapa quando coordenadas são válidas', () => {
    render(<CheckinMap latitude="-23.5505" longitude="-46.6333" />)

    expect(screen.getByTestId('checkin-map')).toBeInTheDocument()
    expect(screen.getByTestId('leaflet-map')).toBeInTheDocument()
    expect(screen.getByTestId('leaflet-marker')).toBeInTheDocument()
  })

  it('deve renderizar placeholder quando coordenadas são inválidas', () => {
    render(<CheckinMap latitude="" longitude="" />)

    expect(screen.queryByTestId('checkin-map')).not.toBeInTheDocument()
    expect(screen.getByText(/selecione a localização para visualizar o mapa/i)).toBeInTheDocument()
  })
})
