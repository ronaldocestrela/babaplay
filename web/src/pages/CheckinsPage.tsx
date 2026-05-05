import { type FormEvent, useMemo, useState } from 'react'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import {
  useCancelCheckin,
  useCheckinsByGameDay,
  useCheckinsByPlayer,
  useCreateCheckin,
} from '@/features/checkin/hooks'
import { checkinFormSchema } from '@/features/checkin/schemas/checkinFormSchema'
import { useCheckinStore } from '@/features/checkin/store/checkinStore'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.CHECKIN_ALREADY_EXISTS]: 'Já existe check-in para este jogador neste dia de jogo.',
  [ERROR_CODES.CHECKIN_OUTSIDE_ALLOWED_RADIUS]: 'Você está fora do raio permitido da associação.',
  [ERROR_CODES.CHECKIN_NOT_FOUND]: 'Check-in não encontrado.',
  [ERROR_CODES.PLAYER_NOT_FOUND]: 'Jogador não encontrado.',
  [ERROR_CODES.PLAYER_INACTIVE]: 'Jogador inativo para realizar check-in.',
  [ERROR_CODES.GAMEDAY_NOT_FOUND]: 'Dia de jogo não encontrado.',
  [ERROR_CODES.FORBIDDEN]: 'Check-ins indisponível para o seu perfil de acesso.',
}

export function CheckinsPage() {
  const {
    selectedGameDayId,
    selectedPlayerId,
    filter,
    setSelectedGameDayId,
    setSelectedPlayerId,
    setFilter,
  } = useCheckinStore()

  const [playerId, setPlayerId] = useState(selectedPlayerId ?? '')
  const [gameDayId, setGameDayId] = useState(selectedGameDayId ?? '')
  const [checkedInAtUtc, setCheckedInAtUtc] = useState(new Date().toISOString())
  const [latitude, setLatitude] = useState('')
  const [longitude, setLongitude] = useState('')
  const [formError, setFormError] = useState<string | null>(null)

  const byGameDay = useCheckinsByGameDay(selectedGameDayId ?? undefined)
  const byPlayer = useCheckinsByPlayer(selectedPlayerId ?? undefined)
  const create = useCreateCheckin()
  const cancel = useCancelCheckin()

  const activeQuery = selectedPlayerId ? byPlayer : byGameDay
  const activeErrorCode =
    create.errorCode ?? cancel.errorCode ?? (activeQuery.error as { response?: { data?: { title?: string } } } | null)?.response?.data?.title ?? null

  const list = useMemo(() => {
    const source = activeQuery.data ?? []
    if (filter === 'all') return source
    return source.filter((item) => item.isActive)
  }, [activeQuery.data, filter])

  const useCurrentLocation = () => {
    setFormError(null)

    if (!navigator.geolocation) {
      setFormError('Geolocalização não suportada neste navegador. Preencha latitude/longitude manualmente.')
      return
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLatitude(String(position.coords.latitude))
        setLongitude(String(position.coords.longitude))
        setCheckedInAtUtc(new Date().toISOString())
      },
      () => {
        setFormError('Não foi possível obter sua localização. Use o preenchimento manual.')
      },
    )
  }

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setFormError(null)

    const parsed = checkinFormSchema.safeParse({
      playerId,
      gameDayId,
      checkedInAtUtc,
      latitude: Number(latitude),
      longitude: Number(longitude),
    })

    if (!parsed.success) {
      setFormError(parsed.error.issues[0]?.message ?? 'Dados inválidos para check-in.')
      return
    }

    setSelectedPlayerId(parsed.data.playerId)
    setSelectedGameDayId(parsed.data.gameDayId)

    create.createCheckin(parsed.data, {
      onSuccess: () => {
        setCheckedInAtUtc(new Date().toISOString())
      },
    })
  }

  const handleCancel = (id: string, checkinPlayerId: string, checkinGameDayId: string) => {
    cancel.cancelCheckin({ id, playerId: checkinPlayerId, gameDayId: checkinGameDayId })
  }

  const loadingMessage = selectedPlayerId
    ? 'Carregando check-ins do jogador...'
    : 'Carregando check-ins por dia de jogo...'

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header>
        <h1 className="text-2xl font-semibold text-on-surface">Check-ins</h1>
        <p className="text-sm text-on-surface-variant">
          Registro e acompanhamento de presença por dia de jogo
        </p>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 space-y-4">
        <h2 className="text-lg font-medium text-on-surface">Novo check-in</h2>

        <form className="grid grid-cols-1 md:grid-cols-2 gap-4" onSubmit={submit} noValidate>
          <div className="space-y-1">
            <label htmlFor="checkin-player-id" className="text-sm text-on-surface">
              PlayerId
            </label>
            <input
              id="checkin-player-id"
              type="text"
              value={playerId}
              onChange={(e) => setPlayerId(e.target.value)}
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
              onChange={(e) => setGameDayId(e.target.value)}
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
              onChange={(e) => setLatitude(e.target.value)}
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
              onChange={(e) => setLongitude(e.target.value)}
              className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
              placeholder="Ex.: -46.6333"
            />
          </div>

          <div className="md:col-span-2 flex flex-wrap gap-2">
            <button
              type="button"
              onClick={useCurrentLocation}
              className="h-10 px-4 rounded-lg border border-outline-variant text-on-surface"
            >
              Usar minha localização
            </button>
            <button
              type="submit"
              disabled={create.isPending || cancel.isPending}
              className="h-10 px-4 rounded-lg border border-primary bg-primary text-white disabled:opacity-60"
            >
              {create.isPending ? 'Registrando...' : 'Registrar check-in'}
            </button>
          </div>
        </form>

        {formError ? <p className="text-sm text-error">{formError}</p> : null}
        {activeErrorCode ? (
          <p className="text-sm text-error">{ERROR_MESSAGES[activeErrorCode] ?? 'Erro ao processar check-in.'}</p>
        ) : null}
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 space-y-4">
        <div className="flex flex-wrap items-center gap-3">
          <button
            type="button"
            onClick={() => setFilter('active')}
            className="h-9 px-3 rounded-lg border border-outline-variant"
          >
            Somente ativos
          </button>
          <button
            type="button"
            onClick={() => setFilter('all')}
            className="h-9 px-3 rounded-lg border border-outline-variant"
          >
            Todos
          </button>
          <button
            type="button"
            onClick={() => setSelectedPlayerId(null)}
            className="h-9 px-3 rounded-lg border border-outline-variant"
          >
            Ver por game day
          </button>
        </div>

        {activeQuery.isLoading ? <p className="text-sm text-on-surface-variant">{loadingMessage}</p> : null}

        {!activeQuery.isLoading && list.length === 0 ? (
          <p className="text-sm text-on-surface-variant">Nenhum check-in encontrado para os filtros atuais.</p>
        ) : null}

        {!activeQuery.isLoading && list.length > 0 ? (
          <ul className="space-y-2">
            {list.map((checkin) => (
              <li
                key={checkin.id}
                className="border border-outline-variant rounded-lg p-3 flex flex-wrap items-center justify-between gap-3"
              >
                <div className="space-y-1 text-sm">
                  <p className="text-on-surface">Player: {checkin.playerId}</p>
                  <p className="text-on-surface-variant">GameDay: {checkin.gameDayId}</p>
                  <p className="text-on-surface-variant">
                    Distância: {checkin.distanceFromAssociationMeters.toFixed(1)}m
                  </p>
                </div>

                <button
                  type="button"
                  onClick={() => handleCancel(checkin.id, checkin.playerId, checkin.gameDayId)}
                  disabled={!checkin.isActive || cancel.isPending}
                  className="h-9 px-3 rounded-lg border border-red-300 text-red-700 disabled:opacity-60"
                >
                  Cancelar
                </button>
              </li>
            ))}
          </ul>
        ) : null}
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <h2 className="text-lg font-medium text-on-surface mb-2">Mapa</h2>
        <p className="text-sm text-on-surface-variant">
          Visualização geográfica inicial habilitada. Próximo slice adiciona provider de mapa com marcador em tempo real.
        </p>
        <div className="mt-3 rounded-lg border border-dashed border-outline-variant h-40 grid place-items-center text-sm text-on-surface-variant">
          Lat {latitude || '--'} | Lon {longitude || '--'}
        </div>
      </section>
    </div>
  )
}
