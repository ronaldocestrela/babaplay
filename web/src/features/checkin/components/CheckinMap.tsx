import { CircleMarker, MapContainer, Popup, TileLayer } from 'react-leaflet'
import 'leaflet/dist/leaflet.css'

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

  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
      <h2 className="text-lg font-medium text-on-surface mb-2">Mapa</h2>
      <p className="text-sm text-on-surface-variant">
        Localização em tempo real do check-in usando provider de mapa.
      </p>

      {hasValidCoordinates ? (
        <div className="mt-3 overflow-hidden rounded-lg border border-outline-variant" data-testid="checkin-map">
          <MapContainer
            center={[parsedLatitude, parsedLongitude]}
            zoom={16}
            scrollWheelZoom={false}
            style={{ height: '16rem', width: '100%' }}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <CircleMarker center={[parsedLatitude, parsedLongitude]} radius={10} pathOptions={{ color: '#0f766e' }}>
              <Popup>
                Check-in atual: Lat {parsedLatitude.toFixed(6)} | Lon {parsedLongitude.toFixed(6)}
              </Popup>
            </CircleMarker>
          </MapContainer>
        </div>
      ) : (
        <div className="mt-3 rounded-lg border border-dashed border-outline-variant h-40 grid place-items-center text-sm text-on-surface-variant">
          Selecione a localização para visualizar o mapa.
        </div>
      )}
    </section>
  )
}
