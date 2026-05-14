import { useMemo, useState, type FormEvent } from 'react'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { TeamForm } from '@/features/teams/components/TeamForm'
import { TeamList } from '@/features/teams/components/TeamList'
import { TeamRosterEditor } from '@/features/teams/components/TeamRosterEditor'
import {
  useCreateTeam,
  useDeleteTeam,
  useTeam,
  useTeamPlayers,
  useTeams,
  useUpdateTeam,
  useUpdateTeamPlayers,
} from '@/features/teams/hooks'
import { teamFormSchema } from '@/features/teams/schemas/teamFormSchema'
import { useTeamStore } from '@/features/teams/store/teamStore'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.TEAM_ALREADY_EXISTS]: 'Já existe um time com este nome.',
  [ERROR_CODES.TEAM_NOT_FOUND]: 'Time não encontrado.',
  [ERROR_CODES.INVALID_NAME]: 'Nome inválido.',
  [ERROR_CODES.INVALID_MAX_PLAYERS]: 'Máximo de jogadores inválido.',
  [ERROR_CODES.TEAM_INVALID_PLAYER_ID]: 'Um ou mais jogadores são inválidos.',
  [ERROR_CODES.TEAM_DUPLICATE_PLAYERS]: 'Jogadores duplicados não são permitidos.',
  [ERROR_CODES.TEAM_PLAYERS_LIMIT_EXCEEDED]: 'Quantidade de jogadores excede o limite do time.',
  [ERROR_CODES.TEAM_PLAYER_NOT_FOUND]: 'Um ou mais jogadores não foram encontrados.',
  [ERROR_CODES.TEAM_GOALKEEPER_REQUIRED]: 'O elenco deve conter pelo menos um goleiro.',
}

function resolveErrorMessage(code: string | null): string | null {
  if (!code) return null
  return ERROR_MESSAGES[code] ?? 'Não foi possível concluir a ação.'
}

export function TeamsPage() {
  const { data: teams = [], isLoading, isError, error } = useTeams()
  const create = useCreateTeam()
  const update = useUpdateTeam()
  const remove = useDeleteTeam()
  const updatePlayers = useUpdateTeamPlayers()
  const playersQuery = useTeamPlayers()

  const {
    search,
    selectedTeamId,
    modalMode,
    isTeamModalOpen,
    isRosterModalOpen,
    setSearch,
    openCreateModal: openCreateModalState,
    openEditModal: openEditModalState,
    closeTeamModal,
    openRosterModal: openRosterModalState,
    closeRosterModal,
  } = useTeamStore()

  const { data: selectedTeamData } = useTeam(selectedTeamId ?? undefined)
  const selectedTeam = selectedTeamData ?? teams.find((team) => team.id === selectedTeamId) ?? null

  const [name, setName] = useState('')
  const [maxPlayers, setMaxPlayers] = useState(11)
  const [formValidationError, setFormValidationError] = useState<string | null>(null)
  const [selectedPlayerIds, setSelectedPlayerIds] = useState<string[]>([])
  const [rosterValidationError, setRosterValidationError] = useState<string | null>(null)

  const filteredTeams = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return teams

    return teams.filter((team) => team.name.toLowerCase().includes(term))
  }, [search, teams])

  const isSubmitting = create.isPending || update.isPending
  const isDeleting = remove.isPending
  const isSavingRoster = updatePlayers.isPending

  const mutationErrorCode =
    create.errorCode ?? update.errorCode ?? remove.errorCode ?? updatePlayers.errorCode ?? null
  const apiErrorCode =
    (error as { response?: { data?: { title?: string } } } | null)?.response?.data?.title ?? null

  const handleDelete = (teamId: string) => {
    if (isDeleting) return

    const confirmed = window.confirm('Deseja remover este time?')
    if (!confirmed) return

    remove.deleteTeam(teamId)
  }

  const handleOpenCreateModal = () => {
    setName('')
    setMaxPlayers(11)
    setFormValidationError(null)
    openCreateModalState()
  }

  const handleOpenEditModal = (teamId: string) => {
    const team = teams.find((item) => item.id === teamId)

    if (team) {
      setName(team.name)
      setMaxPlayers(team.maxPlayers)
      setFormValidationError(null)
    }

    openEditModalState(teamId)
  }

  const handleOpenRosterModal = (teamId: string) => {
    const team = teams.find((item) => item.id === teamId)

    setSelectedPlayerIds(team?.playerIds ?? [])
    setRosterValidationError(null)
    openRosterModalState(teamId)
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const parsed = teamFormSchema.safeParse({
      name,
      maxPlayers,
    })

    if (!parsed.success) {
      setFormValidationError(parsed.error.issues[0]?.message ?? 'Dados inválidos para o time.')
      return
    }

    setFormValidationError(null)

    if (modalMode === 'create') {
      create.createTeam(parsed.data, {
        onSuccess: () => {
          closeTeamModal()
        },
      })
      return
    }

    if (!selectedTeamId) return

    update.updateTeam(
      { id: selectedTeamId, payload: parsed.data },
      {
        onSuccess: () => {
          closeTeamModal()
        },
      },
    )
  }

  const handleTogglePlayer = (playerId: string) => {
    setSelectedPlayerIds((current) =>
      current.includes(playerId)
        ? current.filter((id) => id !== playerId)
        : [...current, playerId],
    )
  }

  const handleSaveRoster = () => {
    if (!selectedTeam) {
      return
    }

    if (selectedPlayerIds.length > selectedTeam.maxPlayers) {
      setRosterValidationError(ERROR_MESSAGES[ERROR_CODES.TEAM_PLAYERS_LIMIT_EXCEEDED])
      return
    }

    if (new Set(selectedPlayerIds).size !== selectedPlayerIds.length) {
      setRosterValidationError(ERROR_MESSAGES[ERROR_CODES.TEAM_DUPLICATE_PLAYERS])
      return
    }

    setRosterValidationError(null)

    updatePlayers.updateTeamPlayers(
      {
        id: selectedTeam.id,
        payload: { playerIds: selectedPlayerIds },
      },
      {
        onSuccess: () => {
          closeRosterModal()
        },
      },
    )
  }

  if (isLoading) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-on-surface-variant">Carregando times...</p>
      </div>
    )
  }

  if (isError) {
    const forbidden = apiErrorCode === ERROR_CODES.FORBIDDEN

    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-error">
          {forbidden
            ? 'Times indisponível no seu perfil de acesso.'
            : 'Não foi possível carregar times.'}
        </p>
      </div>
    )
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-on-surface">Times</h1>
          <p className="text-sm text-on-surface-variant">Gestão de cadastro e elenco</p>
        </div>

        <button
          type="button"
          onClick={handleOpenCreateModal}
          disabled={isSubmitting || isDeleting || isSavingRoster}
          className="h-10 px-4 rounded-lg border border-primary bg-primary text-white text-sm"
        >
          Novo time
        </button>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <label htmlFor="teams-search" className="sr-only">
          Buscar time
        </label>
        <input
          id="teams-search"
          type="text"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Buscar por nome"
          className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
        />
      </section>

      <TeamList
        teams={filteredTeams}
        isDeleting={isDeleting}
        onEdit={handleOpenEditModal}
        onOpenRoster={handleOpenRosterModal}
        onDelete={handleDelete}
      />

      {isTeamModalOpen ? (
        <div className="fixed inset-0 bg-black/40 p-4 flex items-center justify-center z-50">
          <div className="w-full max-w-xl bg-white rounded-xl shadow-xl border border-outline-variant p-6">
            <TeamForm
              mode={modalMode}
              name={name}
              maxPlayers={maxPlayers}
              isSubmitting={isSubmitting}
              validationError={formValidationError}
              apiErrorMessage={resolveErrorMessage(mutationErrorCode)}
              onNameChange={setName}
              onMaxPlayersChange={setMaxPlayers}
              onSubmit={handleSubmit}
              onCancel={closeTeamModal}
            />
          </div>
        </div>
      ) : null}

      {isRosterModalOpen && selectedTeam ? (
        <div className="fixed inset-0 bg-black/40 p-4 flex items-center justify-center z-50">
          <div className="w-full max-w-2xl bg-white rounded-xl shadow-xl border border-outline-variant p-6">
            <TeamRosterEditor
              teamName={selectedTeam.name}
              maxPlayers={selectedTeam.maxPlayers}
              players={(playersQuery.data ?? []).filter((player) => player.isActive)}
              selectedPlayerIds={selectedPlayerIds}
              isSubmitting={isSavingRoster}
              errorMessage={rosterValidationError ?? resolveErrorMessage(mutationErrorCode)}
              onTogglePlayer={handleTogglePlayer}
              onSave={handleSaveRoster}
              onCancel={closeRosterModal}
            />
          </div>
        </div>
      ) : null}
    </div>
  )
}
