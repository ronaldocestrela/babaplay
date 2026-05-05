import { type FormEvent } from 'react'

interface CheckinFormProps {
  playerId: string
  gameDayId: string
  latitude: string
  longitude: string
  isSubmitting: boolean
  formError: string | null
  apiErrorMessage: string | null
  onPlayerIdChange: (value: string) => void
  onGameDayIdChange: (value: string) => void
  onLatitudeChange: (value: string) => void
  onLongitudeChange: (value: string) => void
  onUseCurrentLocation: () => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}

export function CheckinForm({
  playerId,
  gameDayId,
  latitude,
  longitude,
  isSubmitting,
  formError,
  apiErrorMessage,
  onPlayerIdChange,
  onGameDayIdChange,
  onLatitudeChange,
  onLongitudeChange,
  onUseCurrentLocation,
  onSubmit,
}: CheckinFormProps) {
  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 space-y-4">
      <h2 className="text-lg font-medium text-on-surface">Novo check-in</h2>

      <form className="grid grid-cols-1 md:grid-cols-2 gap-4" onSubmit={onSubmit} noValidate>
        <div className="space-y-1">
          <label htmlFor="checkin-player-id" className="text-sm text-on-surface">
            PlayerId
          </label>
          <input
            id="checkin-player-id"
            type="text"
            value={playerId}
            onChange={(event) => onPlayerIdChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
            placeholder="UUID do jogador"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="checkin-gameday-id" className="text-sm text-on-surface">
            GameDayId
          </label>
          <input
            id="checkin-gameday-id"
            type="text"
            value={gameDayId}
            onChange={(event) => onGameDayIdChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
            placeholder="UUID do dia de jogo"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="checkin-latitude" className="text-sm text-on-surface">
            Latitude
          </label>
          <input
            id="checkin-latitude"
            type="number"
            step="any"
            value={latitude}
            onChange={(event) => onLatitudeChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
            placeholder="Ex.: -23.5505"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="checkin-longitude" className="text-sm text-on-surface">
            Longitude
          </label>
          <input
            id="checkin-longitude"
            type="number"
            step="any"
            value={longitude}
            onChange={(event) => onLongitudeChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
            placeholder="Ex.: -46.6333"
          />
        </div>

        <div className="md:col-span-2 flex flex-wrap gap-2">
          <button
            type="button"
            onClick={onUseCurrentLocation}
            className="h-10 px-4 rounded-lg border border-outline-variant text-on-surface"
          >
            Usar minha localização
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="h-10 px-4 rounded-lg border border-primary bg-primary text-white disabled:opacity-60"
          >
            {isSubmitting ? 'Registrando...' : 'Registrar check-in'}
          </button>
        </div>
      </form>

      {formError ? <p className="text-sm text-error">{formError}</p> : null}
      {apiErrorMessage ? <p className="text-sm text-error">{apiErrorMessage}</p> : null}
    </section>
  )
}
