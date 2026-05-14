import { CircleMarker, MapContainer, TileLayer, useMapEvents } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'

interface AssociationLocationMapProps {
  latitude: string
  longitude: string
  onCoordinateChange: (latitude: string, longitude: string) => void
}

function MapClickHandler({
  onCoordinateChange,
}: {
  onCoordinateChange: (latitude: string, longitude: string) => void
}) {
  useMapEvents({
    click: (event) => {
      onCoordinateChange(event.latlng.lat.toFixed(6), event.latlng.lng.toFixed(6))
    },
  })

  return null
}

function parseCoordinate(value: string): number | null {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : null
}

export function AssociationLocationMap({
  latitude,
  longitude,
  onCoordinateChange,
}: AssociationLocationMapProps) {
  const parsedLatitude = parseCoordinate(latitude)
  const parsedLongitude = parseCoordinate(longitude)

  const hasValidCoordinates =
    parsedLatitude !== null &&
    parsedLongitude !== null &&
    Math.abs(parsedLatitude) <= 90 &&
    Math.abs(parsedLongitude) <= 180

  const center: [number, number] = hasValidCoordinates
    ? [parsedLatitude, parsedLongitude]
    : [-23.5505, -46.6333]

  const mapKey = `${center[0]}-${center[1]}`

  return (
    <section className="rounded-lg border border-gray-200 bg-white p-3">
      <p className="text-sm text-gray-600">
        Clique no mapa para ajustar latitude e longitude da associação.
      </p>
      <div className="mt-3 overflow-hidden rounded-lg border border-gray-200" data-testid="association-location-map">
        <MapContainer key={mapKey} center={center} zoom={15} scrollWheelZoom style={{ height: '16rem', width: '100%' }}>
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <MapClickHandler onCoordinateChange={onCoordinateChange} />
          {hasValidCoordinates ? (
            <CircleMarker center={[parsedLatitude, parsedLongitude]} radius={10} pathOptions={{ color: '#2563eb' }} />
          ) : null}
        </MapContainer>
      </div>
    </section>
  )
}
