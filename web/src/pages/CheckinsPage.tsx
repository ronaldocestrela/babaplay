import { type FormEvent, useMemo, useState } from 'react'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { CheckinForm } from '@/features/checkin/components/CheckinForm'
import { CheckinList } from '@/features/checkin/components/CheckinList'
import { CheckinMap } from '@/features/checkin/components/CheckinMap'
import {
  useCancelCheckin,
  useCheckinGameDays,
  useCheckinPlayers,
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

function resolveErrorMessage(errorCode: string | null): string {
  if (!errorCode) {
    return 'Não foi possível concluir a ação.'
  }

  return ERROR_MESSAGES[errorCode] ?? 'Não foi possível concluir a ação.'
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
  const [actionFeedback, setActionFeedback] = useState<{
    type: 'success' | 'error'
    message: string
  } | null>(null)
  const [cancellingCheckinId, setCancellingCheckinId] = useState<string | null>(null)

  const byGameDay = useCheckinsByGameDay(selectedGameDayId ?? undefined)
  const byPlayer = useCheckinsByPlayer(selectedPlayerId ?? undefined)
  const playersQuery = useCheckinPlayers()
  const gameDaysQuery = useCheckinGameDays()
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
    setActionFeedback(null)

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
        setActionFeedback({
          type: 'success',
          message: 'Check-in registrado com sucesso.',
        })
      },
      onError: (error) => {
        const errorCode = getErrorCode(error)
        setActionFeedback({
          type: 'error',
          message: resolveErrorMessage(errorCode),
        })
      },
    })
  }

  const handleCancel = (id: string, checkinPlayerId: string, checkinGameDayId: string) => {
    setActionFeedback(null)
    setCancellingCheckinId(id)

    cancel.cancelCheckin(
      { id, playerId: checkinPlayerId, gameDayId: checkinGameDayId },
      {
        onSuccess: () => {
          setActionFeedback({
            type: 'success',
            message: 'Check-in cancelado com sucesso.',
          })
        },
        onError: (error) => {
          const errorCode = getErrorCode(error)
          setActionFeedback({
            type: 'error',
            message: resolveErrorMessage(errorCode),
          })
        },
        onSettled: () => {
          setCancellingCheckinId(null)
        },
      },
    )
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

      {actionFeedback ? (
        <div
          className={
            actionFeedback.type === 'success'
              ? 'rounded-lg border border-green-300 bg-green-50 px-4 py-3 text-sm text-green-800'
              : 'rounded-lg border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-800'
          }
          role="status"
        >
          {actionFeedback.message}
        </div>
      ) : null}

      <CheckinForm
        playerId={playerId}
        gameDayId={gameDayId}
        playerOptions={(playersQuery.data ?? []).filter((player) => player.isActive)}
        gameDayOptions={gameDaysQuery.data ?? []}
        isPlayersLoading={playersQuery.isLoading}
        isGameDaysLoading={gameDaysQuery.isLoading}
        latitude={latitude}
        longitude={longitude}
        isSubmitting={create.isPending || cancel.isPending}
        formError={formError}
        apiErrorMessage={activeErrorCode ? (ERROR_MESSAGES[activeErrorCode] ?? 'Erro ao processar check-in.') : null}
        onPlayerIdChange={setPlayerId}
        onGameDayIdChange={setGameDayId}
        onLatitudeChange={setLatitude}
        onLongitudeChange={setLongitude}
        onUseCurrentLocation={useCurrentLocation}
        onSubmit={submit}
      />

      <CheckinList
        list={list}
        filter={filter}
        isLoading={activeQuery.isLoading}
        isCancelling={cancel.isPending}
        cancellingCheckinId={cancellingCheckinId}
        loadingMessage={loadingMessage}
        onFilterChange={setFilter}
        onViewByGameDay={() => setSelectedPlayerId(null)}
        onCancel={handleCancel}
      />

      <CheckinMap latitude={latitude} longitude={longitude} />
    </div>
  )
}
