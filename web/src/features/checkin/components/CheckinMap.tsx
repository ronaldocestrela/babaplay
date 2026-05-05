interface CheckinMapProps {
  latitude: string
  longitude: string
}

export function CheckinMap({ latitude, longitude }: CheckinMapProps) {
  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
      <h2 className="text-lg font-medium text-on-surface mb-2">Mapa</h2>
      <p className="text-sm text-on-surface-variant">
        Visualização geográfica inicial habilitada. Próximo slice adiciona provider de mapa com marcador em tempo real.
      </p>
      <div className="mt-3 rounded-lg border border-dashed border-outline-variant h-40 grid place-items-center text-sm text-on-surface-variant">
        Lat {latitude || '--'} | Lon {longitude || '--'}
      </div>
    </section>
  )
}
