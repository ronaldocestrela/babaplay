import { type FormEvent, useMemo, useState } from 'react'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { CheckinForm } from '@/features/checkin/components/CheckinForm'
import { CheckinList } from '@/features/checkin/components/CheckinList'
import { CheckinMap } from '@/features/checkin/components/CheckinMap'
import {
  useCheckinGameDays,
  useCheckinPlayers,
  useCheckinsByPlayer,
  useCreateCheckin,
} from '@/features/checkin/hooks'
import { checkinFormSchema } from '@/features/checkin/schemas/checkinFormSchema'
import { useAuthStore } from '@/features/auth/store/authStore'

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
  const currentUser = useAuthStore((s) => s.currentUser)
  const [gameDayId, setGameDayId] = useState('')
  const [checkedInAtUtc, setCheckedInAtUtc] = useState(new Date().toISOString())
  const [latitude, setLatitude] = useState('')
  const [longitude, setLongitude] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [actionFeedback, setActionFeedback] = useState<{
    type: 'success' | 'error'
    message: string
  } | null>(null)

  const playersQuery = useCheckinPlayers()
  const gameDaysQuery = useCheckinGameDays()
  const create = useCreateCheckin()
  const currentPlayer = useMemo(
    () =>
      (playersQuery.data ?? []).find(
        (player) => player.isActive && player.userId === currentUser?.id,
      ) ?? null,
    [currentUser?.id, playersQuery.data],
  )

  const byPlayer = useCheckinsByPlayer(currentPlayer?.id)

  const activeErrorCode =
    create.errorCode ?? (byPlayer.error as { response?: { data?: { title?: string } } } | null)?.response?.data?.title ?? null

  const list = useMemo(() => {
    const source = byPlayer.data ?? []
    return source.filter((item) => item.isActive)
  }, [byPlayer.data])

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

    if (!currentPlayer?.id) {
      setFormError('Voce ainda nao possui perfil de jogador ativo para realizar check-in.')
      return
    }

    const parsed = checkinFormSchema.safeParse({
      playerId: currentPlayer.id,
      gameDayId,
      checkedInAtUtc,
      latitude: Number(latitude),
      longitude: Number(longitude),
    })

    if (!parsed.success) {
      setFormError(parsed.error.issues[0]?.message ?? 'Dados inválidos para check-in.')
      return
    }

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
  const loadingMessage = 'Carregando seus check-ins...'

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
        gameDayId={gameDayId}
        gameDayOptions={gameDaysQuery.data ?? []}
        isGameDaysLoading={gameDaysQuery.isLoading}
        latitude={latitude}
        longitude={longitude}
        isSubmitting={create.isPending}
        formError={formError}
        apiErrorMessage={activeErrorCode ? (ERROR_MESSAGES[activeErrorCode] ?? 'Erro ao processar check-in.') : null}
        onGameDayIdChange={setGameDayId}
        onLatitudeChange={setLatitude}
        onLongitudeChange={setLongitude}
        onUseCurrentLocation={useCurrentLocation}
        onSubmit={submit}
      />

      <CheckinList
        list={list}
        isLoading={byPlayer.isLoading}
        loadingMessage={loadingMessage}
      />

      <CheckinMap latitude={latitude} longitude={longitude} />
    </div>
  )
}
