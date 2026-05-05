import { useMemo, useState, type FormEvent } from 'react'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { MatchForm } from '@/features/matches/components/MatchForm'
import { MatchList } from '@/features/matches/components/MatchList'
import {
  useChangeMatchStatus,
  useCreateMatch,
  useDeleteMatch,
  useMatches,
  useMatchGameDays,
  useMatchTeams,
  useUpdateMatch,
} from '@/features/matches/hooks'
import { matchFormSchema } from '@/features/matches/schemas/matchFormSchema'
import { useMatchStore } from '@/features/matches/store/matchStore'
import type { MatchStatus } from '@/features/matches/types'
import { getErrorCode } from '@/core/utils/getErrorCode'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.MATCH_NOT_FOUND]: 'Partida não encontrada.',
  [ERROR_CODES.MATCH_ALREADY_EXISTS]: 'Já existe uma partida com esse confronto para o dia informado.',
  [ERROR_CODES.TEAMS_MUST_BE_DIFFERENT]: 'Times mandante e visitante devem ser diferentes.',
  [ERROR_CODES.GAMEDAY_NOT_FOUND]: 'Dia de jogo não encontrado.',
  [ERROR_CODES.GAMEDAY_PAST]: 'Não é possível cadastrar partida para dia de jogo passado.',
  [ERROR_CODES.TEAM_NOT_FOUND]: 'Um ou mais times não foram encontrados.',
  [ERROR_CODES.INVALID_STATUS_TRANSITION]: 'Transição de status inválida para esta partida.',
}

function resolveErrorMessage(code: string | null): string | null {
  if (!code) return null
  return ERROR_MESSAGES[code] ?? 'Não foi possível concluir a ação.'
}

export function MatchesPage() {
  const { data: matches = [], isLoading, isError, error } = useMatches()
  const { data: gameDays = [] } = useMatchGameDays()
  const { data: teams = [] } = useMatchTeams()

  const create = useCreateMatch()
  const update = useUpdateMatch()
  const remove = useDeleteMatch()
  const status = useChangeMatchStatus()

  const {
    search,
    selectedMatchId,
    modalMode,
    isModalOpen,
    setSearch,
    openCreateModal,
    openEditModal,
    closeModal,
  } = useMatchStore()

  const selectedMatch = useMemo(
    () => matches.find((item) => item.id === selectedMatchId) ?? null,
    [matches, selectedMatchId],
  )

  const [formValues, setFormValues] = useState({
    gameDayId: '',
    homeTeamId: '',
    awayTeamId: '',
    description: '',
  })
  const [formValidationError, setFormValidationError] = useState<string | null>(null)
  const [changingStatusMatchId, setChangingStatusMatchId] = useState<string | null>(null)

  const resetFormState = () => {
    setFormValues({ gameDayId: '', homeTeamId: '', awayTeamId: '', description: '' })
    setFormValidationError(null)
  }

  const handleCloseModal = () => {
    resetFormState()
    closeModal()
  }

  const handleOpenCreateModal = () => {
    resetFormState()
    openCreateModal()
  }

  const handleOpenEditModal = (matchId: string) => {
    const match = matches.find((item) => item.id === matchId)

    if (match) {
      setFormValues({
        gameDayId: match.gameDayId,
        homeTeamId: match.homeTeamId,
        awayTeamId: match.awayTeamId,
        description: match.description ?? '',
      })
      setFormValidationError(null)
    }

    openEditModal(matchId)
  }

  const filteredMatches = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return matches

    return matches.filter((match) => {
      const homeTeam = teams.find((item) => item.id === match.homeTeamId)?.name ?? ''
      const awayTeam = teams.find((item) => item.id === match.awayTeamId)?.name ?? ''

      return (
        homeTeam.toLowerCase().includes(term) ||
        awayTeam.toLowerCase().includes(term) ||
        match.status.toLowerCase().includes(term)
      )
    })
  }, [matches, search, teams])

  const isSubmitting = create.isPending || update.isPending
  const isDeleting = remove.isPending

  const mutationErrorCode =
    create.errorCode ?? update.errorCode ?? remove.errorCode ?? status.errorCode ?? null

  const apiErrorCode = getErrorCode(error)

  const handleDelete = (matchId: string) => {
    if (isDeleting) return

    const confirmed = window.confirm('Deseja remover esta partida?')
    if (!confirmed) return

    remove.deleteMatch(matchId)
  }

  const handleStatusChange = (matchId: string, nextStatus: MatchStatus) => {
    setChangingStatusMatchId(matchId)

    status.changeMatchStatus(
      {
        id: matchId,
        payload: { status: nextStatus },
      },
      {
        onSettled: () => {
          setChangingStatusMatchId(null)
        },
      },
    )
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const parsed = matchFormSchema.safeParse(formValues)
    if (!parsed.success) {
      setFormValidationError(parsed.error.issues[0]?.message ?? 'Dados inválidos para partida.')
      return
    }

    setFormValidationError(null)

    if (modalMode === 'create') {
      create.createMatch(
        {
          gameDayId: parsed.data.gameDayId,
          homeTeamId: parsed.data.homeTeamId,
          awayTeamId: parsed.data.awayTeamId,
          description: parsed.data.description ?? null,
        },
        {
          onSuccess: () => {
            handleCloseModal()
          },
        },
      )
      return
    }

    if (!selectedMatch) {
      return
    }

    update.updateMatch(
      {
        id: selectedMatch.id,
        payload: {
          gameDayId: parsed.data.gameDayId,
          homeTeamId: parsed.data.homeTeamId,
          awayTeamId: parsed.data.awayTeamId,
          description: parsed.data.description ?? null,
        },
      },
      {
        onSuccess: () => {
          handleCloseModal()
        },
      },
    )
  }

  if (isLoading) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-on-surface-variant">Carregando partidas...</p>
      </div>
    )
  }

  if (isError) {
    const forbidden = apiErrorCode === ERROR_CODES.FORBIDDEN

    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-error">
          {forbidden
            ? 'Partidas indisponível no seu perfil de acesso.'
            : 'Não foi possível carregar partidas.'}
        </p>
      </div>
    )
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-on-surface">Partidas</h1>
          <p className="text-sm text-on-surface-variant">Gestão de confrontos e status do jogo</p>
        </div>

        <button
          type="button"
          onClick={handleOpenCreateModal}
          disabled={isSubmitting || isDeleting}
          className="h-10 px-4 rounded-lg border border-primary bg-primary text-white text-sm"
        >
          Nova partida
        </button>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <label htmlFor="matches-search" className="sr-only">
          Buscar partida
        </label>
        <input
          id="matches-search"
          type="text"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Buscar por time ou status"
          className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
        />
      </section>

      <MatchList
        matches={filteredMatches}
        teams={teams}
        isDeleting={isDeleting}
        changingStatusMatchId={changingStatusMatchId}
        onEdit={handleOpenEditModal}
        onDelete={handleDelete}
        onStatusChange={handleStatusChange}
      />

      {isModalOpen ? (
        <div className="fixed inset-0 bg-black/40 p-4 flex items-center justify-center z-50">
          <div className="w-full max-w-xl bg-white rounded-xl shadow-xl border border-outline-variant p-6">
            <MatchForm
              mode={modalMode}
              values={formValues}
              gameDays={gameDays}
              teams={teams}
              isSubmitting={isSubmitting}
              validationError={formValidationError}
              apiErrorMessage={resolveErrorMessage(mutationErrorCode)}
              onValueChange={(field, value) =>
                setFormValues((current) => ({
                  ...current,
                  [field]: value,
                }))
              }
              onSubmit={handleSubmit}
              onCancel={handleCloseModal}
            />
          </div>
        </div>
      ) : null}
    </div>
  )
}
