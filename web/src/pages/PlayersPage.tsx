import { useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import {
  useCreatePlayer,
  useDeletePlayer,
  usePlayers,
  usePositions,
  useUpdatePlayer,
  useUpdatePlayerPositions,
} from '@/features/players/hooks'
import { playerFormSchema, type PlayerFormValues } from '@/features/players/schemas/playerFormSchema'
import { usePlayerStore } from '@/features/players/store/playerStore'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.PLAYER_ALREADY_EXISTS]: 'Já existe um jogador para este usuário.',
  [ERROR_CODES.INVALID_NAME]: 'Nome inválido.',
  [ERROR_CODES.USER_NOT_FOUND]: 'Usuário não encontrado.',
  [ERROR_CODES.PLAYER_NOT_FOUND]: 'Jogador não encontrado.',
  [ERROR_CODES.POSITION_NOT_FOUND]: 'Uma ou mais posições não foram encontradas.',
  [ERROR_CODES.POSITIONS_LIMIT_EXCEEDED]: 'Máximo de 3 posições por jogador.',
  [ERROR_CODES.DUPLICATE_POSITIONS]: 'Posições duplicadas não são permitidas.',
  [ERROR_CODES.INVALID_POSITION_ID]: 'Uma ou mais posições são inválidas.',
}

function toNullable(value: string | undefined): string | null {
  if (!value) return null
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

export function PlayersPage() {
  const { data: players = [], isLoading, isError, error } = usePlayers()
  const { data: positions = [] } = usePositions()
  const create = useCreatePlayer()
  const update = useUpdatePlayer()
  const remove = useDeletePlayer()
  const updatePositions = useUpdatePlayerPositions()

  const {
    search,
    selectedPlayerId,
    modalMode,
    isModalOpen,
    setSearch,
    openCreateModal,
    openEditModal,
    closeModal,
  } = usePlayerStore()

  const selectedPlayer = useMemo(
    () => players.find((player) => player.id === selectedPlayerId) ?? null,
    [players, selectedPlayerId],
  )

  const filteredPlayers = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return players

    return players.filter((player) => {
      const byName = player.name.toLowerCase().includes(term)
      const byNick = (player.nickname ?? '').toLowerCase().includes(term)
      return byName || byNick
    })
  }, [players, search])

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PlayerFormValues>({
    resolver: zodResolver(playerFormSchema),
    defaultValues: {
      userId: '',
      name: '',
      nickname: '',
      phone: '',
      dateOfBirth: '',
      positionIds: [],
    },
  })

  useEffect(() => {
    if (!isModalOpen) {
      reset({
        userId: '',
        name: '',
        nickname: '',
        phone: '',
        dateOfBirth: '',
        positionIds: [],
      })
      return
    }

    if (modalMode === 'edit' && selectedPlayer) {
      reset({
        userId: selectedPlayer.userId,
        name: selectedPlayer.name,
        nickname: selectedPlayer.nickname ?? '',
        phone: selectedPlayer.phone ?? '',
        dateOfBirth: selectedPlayer.dateOfBirth ?? '',
        positionIds: [],
      })
      return
    }

    reset({
      userId: '',
      name: '',
      nickname: '',
      phone: '',
      dateOfBirth: '',
      positionIds: [],
    })
  }, [isModalOpen, modalMode, reset, selectedPlayer])

  const mutationErrorCode =
    create.errorCode ?? update.errorCode ?? remove.errorCode ?? updatePositions.errorCode ?? null
  const isSubmitting = create.isPending || update.isPending || updatePositions.isPending
  const isDeleting = remove.isPending

  const apiErrorCode = getErrorCode(error)

  const onSubmit = (values: PlayerFormValues) => {
    const positionIds = values.positionIds

    if (modalMode === 'create') {
      if (!values.userId) {
        return
      }

      create.createPlayer(
        {
          userId: values.userId,
          name: values.name,
          nickname: toNullable(values.nickname),
          phone: toNullable(values.phone),
          dateOfBirth: toNullable(values.dateOfBirth),
        },
        {
          onSuccess: (created) => {
            if (positionIds.length > 0) {
              updatePositions.updatePlayerPositions(
                { id: created.id, payload: { positionIds } },
                {
                  onSuccess: () => {
                    closeModal()
                  },
                },
              )
              return
            }

            closeModal()
          },
        },
      )

      return
    }

    if (!selectedPlayer) {
      return
    }

    update.updatePlayer(
      {
        id: selectedPlayer.id,
        payload: {
          name: values.name,
          nickname: toNullable(values.nickname),
          phone: toNullable(values.phone),
          dateOfBirth: toNullable(values.dateOfBirth),
        },
      },
      {
        onSuccess: () => {
          if (positionIds.length > 0) {
            updatePositions.updatePlayerPositions(
              { id: selectedPlayer.id, payload: { positionIds } },
              {
                onSuccess: () => {
                  closeModal()
                },
              },
            )
            return
          }

          closeModal()
        },
      },
    )
  }

  const handleDelete = (playerId: string) => {
    if (isDeleting) {
      return
    }

    const confirmed = window.confirm('Deseja remover este jogador?')
    if (!confirmed) return

    remove.deletePlayer(playerId)
  }

  if (isLoading) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-on-surface-variant">Carregando jogadores...</p>
      </div>
    )
  }

  if (isError) {
    const forbidden = apiErrorCode === ERROR_CODES.FORBIDDEN

    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-error">
          {forbidden
            ? 'Jogadores indisponível no seu perfil de acesso.'
            : 'Não foi possível carregar jogadores.'}
        </p>
      </div>
    )
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-on-surface">Jogadores</h1>
          <p className="text-sm text-on-surface-variant">Gestão de cadastro e posições</p>
        </div>

        <button
          type="button"
          onClick={openCreateModal}
          disabled={isSubmitting || isDeleting}
          className="h-10 px-4 rounded-lg border border-primary bg-primary text-white text-sm"
        >
          Novo jogador
        </button>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <label htmlFor="players-search" className="sr-only">
          Buscar jogador
        </label>
        <input
          id="players-search"
          type="text"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Buscar por nome ou apelido"
          className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
        />
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden">
        {filteredPlayers.length === 0 ? (
          <div className="p-6 text-sm text-on-surface-variant">Nenhum jogador encontrado.</div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-surface-container-high text-on-surface-variant">
              <tr>
                <th className="text-left p-3 font-medium">Nome</th>
                <th className="text-left p-3 font-medium">Apelido</th>
                <th className="text-left p-3 font-medium">Telefone</th>
                <th className="text-left p-3 font-medium">Status</th>
                <th className="text-right p-3 font-medium">Ações</th>
              </tr>
            </thead>
            <tbody>
              {filteredPlayers.map((player) => (
                <tr key={player.id} className="border-t border-outline-variant">
                  <td className="p-3 text-on-surface">{player.name}</td>
                  <td className="p-3 text-on-surface-variant">{player.nickname ?? '-'}</td>
                  <td className="p-3 text-on-surface-variant">{player.phone ?? '-'}</td>
                  <td className="p-3 text-on-surface-variant">{player.isActive ? 'Ativo' : 'Inativo'}</td>
                  <td className="p-3">
                    <div className="flex justify-end gap-2">
                      <button
                        type="button"
                        onClick={() => openEditModal(player.id)}
                        disabled={isSubmitting || isDeleting}
                        className="px-3 py-1.5 rounded-lg border border-outline-variant text-on-surface"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => handleDelete(player.id)}
                        disabled={isDeleting || isSubmitting}
                        className="px-3 py-1.5 rounded-lg border border-red-300 text-red-700"
                      >
                        {isDeleting ? 'Excluindo...' : 'Excluir'}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      {isModalOpen ? (
        <div className="fixed inset-0 bg-black/40 p-4 flex items-center justify-center z-50">
          <div className="w-full max-w-2xl bg-white rounded-xl shadow-xl border border-outline-variant">
            <div className="px-6 py-4 border-b border-outline-variant flex items-center justify-between">
              <h2 className="text-lg font-semibold text-on-surface">
                {modalMode === 'create' ? 'Novo jogador' : 'Editar jogador'}
              </h2>
              <button
                type="button"
                onClick={closeModal}
                disabled={isSubmitting}
                className="text-on-surface-variant hover:text-on-surface"
              >
                Fechar
              </button>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4" noValidate>
              {modalMode === 'create' ? (
                <div className="space-y-1">
                  <label htmlFor="userId" className="text-sm text-on-surface">
                    UserId
                  </label>
                  <input
                    id="userId"
                    type="text"
                    {...register('userId')}
                    className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
                  />
                </div>
              ) : null}

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-1 md:col-span-2">
                  <label htmlFor="name" className="text-sm text-on-surface">
                    Nome
                  </label>
                  <input
                    id="name"
                    type="text"
                    {...register('name')}
                    className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
                  />
                  {errors.name ? (
                    <p role="alert" className="text-xs text-error">
                      {errors.name.message}
                    </p>
                  ) : null}
                </div>

                <div className="space-y-1">
                  <label htmlFor="nickname" className="text-sm text-on-surface">
                    Apelido
                  </label>
                  <input
                    id="nickname"
                    type="text"
                    {...register('nickname')}
                    className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
                  />
                </div>

                <div className="space-y-1">
                  <label htmlFor="phone" className="text-sm text-on-surface">
                    Telefone
                  </label>
                  <input
                    id="phone"
                    type="text"
                    {...register('phone')}
                    className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
                  />
                </div>

                <div className="space-y-1 md:col-span-2">
                  <label htmlFor="dateOfBirth" className="text-sm text-on-surface">
                    Data de nascimento
                  </label>
                  <input
                    id="dateOfBirth"
                    type="date"
                    {...register('dateOfBirth')}
                    className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
                  />
                  {errors.dateOfBirth ? (
                    <p role="alert" className="text-xs text-error">
                      {errors.dateOfBirth.message}
                    </p>
                  ) : null}
                </div>

                <div className="space-y-2 md:col-span-2">
                  <p className="text-sm text-on-surface">Posições (máximo 3)</p>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    {positions.map((position) => (
                      <label
                        key={position.id}
                        className="flex items-center gap-2 text-sm text-on-surface-variant"
                      >
                        <input
                          type="checkbox"
                          value={position.id}
                          {...register('positionIds')}
                        />
                        {position.name}
                      </label>
                    ))}
                  </div>
                  {errors.positionIds ? (
                    <p role="alert" className="text-xs text-error">
                      {errors.positionIds.message}
                    </p>
                  ) : null}
                </div>
              </div>

              {mutationErrorCode ? (
                <p role="alert" className="text-sm text-error" aria-live="polite">
                  {ERROR_MESSAGES[mutationErrorCode] ?? 'Não foi possível salvar o jogador.'}
                </p>
              ) : null}

              <div className="flex items-center justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={closeModal}
                  disabled={isSubmitting}
                  className="h-10 px-4 rounded-lg border border-outline-variant"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="h-10 px-4 rounded-lg border border-primary bg-primary text-white"
                >
                  {isSubmitting ? 'Salvando...' : 'Salvar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      ) : null}
    </div>
  )
}