import type { ComponentType, CSSProperties, ReactNode } from 'react'
import { CircleMarker, MapContainer, Popup, TileLayer } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'

type LatLngTuple = [number, number]

interface SafeMapContainerProps {
  center: LatLngTuple
  zoom: number
  scrollWheelZoom?: boolean
  style?: CSSProperties
  children?: ReactNode
}

interface SafeTileLayerProps {
  attribution?: string
  url: string
}

interface SafeCircleMarkerProps {
  center: LatLngTuple
  radius?: number
  pathOptions?: { color?: string }
  children?: ReactNode
}

const SafeMapContainer = MapContainer as unknown as ComponentType<SafeMapContainerProps>
const SafeTileLayer = TileLayer as unknown as ComponentType<SafeTileLayerProps>
const SafeCircleMarker = CircleMarker as unknown as ComponentType<SafeCircleMarkerProps>

interface CheckinMapProps {
  latitude: string
  longitude: string
}

export function CheckinMap({ latitude, longitude }: CheckinMapProps) {
  const hasLatitudeInput = latitude.trim().length > 0
  const hasLongitudeInput = longitude.trim().length > 0
  const parsedLatitude = Number(latitude)
  const parsedLongitude = Number(longitude)
  const hasValidCoordinates =
    hasLatitudeInput &&
    hasLongitudeInput &&
    Number.isFinite(parsedLatitude) &&
    Number.isFinite(parsedLongitude) &&
    Math.abs(parsedLatitude) <= 90 &&
    Math.abs(parsedLongitude) <= 180

  const center: LatLngTuple = [parsedLatitude, parsedLongitude]

  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
      <h2 className="text-lg font-medium text-on-surface mb-2">Mapa</h2>
      <p className="text-sm text-on-surface-variant">
        Localização em tempo real do check-in usando provider de mapa.
      </p>

      {hasValidCoordinates ? (
        <div className="mt-3 overflow-hidden rounded-lg border border-outline-variant" data-testid="checkin-map">
          <SafeMapContainer
            center={center}
            zoom={16}
            scrollWheelZoom={false}
            style={{ height: '16rem', width: '100%' }}
          >
            <SafeTileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <SafeCircleMarker center={center} radius={10} pathOptions={{ color: '#0f766e' }}>
              <Popup>
                Check-in atual: Lat {parsedLatitude.toFixed(6)} | Lon {parsedLongitude.toFixed(6)}
              </Popup>
            </SafeCircleMarker>
          </SafeMapContainer>
        </div>
      ) : (
        <div className="mt-3 rounded-lg border border-dashed border-outline-variant h-40 grid place-items-center text-sm text-on-surface-variant">
          Selecione a localização para visualizar o mapa.
        </div>
      )}
    </section>
  )
}
