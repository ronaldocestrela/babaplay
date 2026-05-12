import { useMemo, useState, type FormEvent } from 'react'
import { useAuthStore } from '@/features/auth/store/authStore'
import { isTenantAdmin } from '@/features/auth/utils/tenantAccess'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { PositionForm } from '@/features/positions/components/PositionForm'
import { PositionList } from '@/features/positions/components/PositionList'
import {
  useCreatePosition,
  useDeletePosition,
  usePositions,
  useUpdatePosition,
} from '@/features/positions/hooks'
import { positionFormSchema } from '@/features/positions/schemas/positionFormSchema'
import { usePositionStore } from '@/features/positions/store/positionStore'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.POSITION_ALREADY_EXISTS]: 'Já existe uma posição com este código.',
  [ERROR_CODES.POSITION_NOT_FOUND]: 'Posição não encontrada.',
  [ERROR_CODES.POSITION_IN_USE]: 'Não é possível excluir posição em uso por jogador.',
  [ERROR_CODES.INVALID_CODE]: 'Código inválido.',
  [ERROR_CODES.INVALID_NAME]: 'Nome inválido.',
  [ERROR_CODES.FORBIDDEN]: 'Somente admin pode cadastrar e alterar posições.',
}

function resolveErrorMessage(code: string | null): string | null {
  if (!code) return null
  return ERROR_MESSAGES[code] ?? 'Não foi possível concluir a ação.'
}

export function PositionsPage() {
  const currentUser = useAuthStore((s) => s.currentUser)
  const currentTenant = useAuthStore((s) => s.currentTenant)
  const canManage = isTenantAdmin(currentUser, currentTenant)

  const { data: positions = [], isLoading, isError, error } = usePositions()
  const create = useCreatePosition()
  const update = useUpdatePosition()
  const remove = useDeletePosition()

  const {
    search,
    selectedPositionId,
    modalMode,
    isPositionModalOpen,
    setSearch,
    openCreateModal: openCreateModalState,
    openEditModal: openEditModalState,
    closePositionModal,
  } = usePositionStore()

  const selectedPosition = positions.find((position) => position.id === selectedPositionId) ?? null

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [formValidationError, setFormValidationError] = useState<string | null>(null)

  const filteredPositions = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return positions

    return positions.filter(
      (position) =>
        position.code.toLowerCase().includes(term) ||
        position.name.toLowerCase().includes(term) ||
        (position.description ?? '').toLowerCase().includes(term),
    )
  }, [positions, search])

  const isSubmitting = create.isPending || update.isPending
  const isDeleting = remove.isPending

  const mutationErrorCode = create.errorCode ?? update.errorCode ?? remove.errorCode ?? null
  const apiErrorCode =
    (error as { response?: { data?: { title?: string } } } | null)?.response?.data?.title ?? null

  const handleDelete = (positionId: string) => {
    if (!canManage || isDeleting) return

    const confirmed = window.confirm('Deseja remover esta posição?')
    if (!confirmed) return

    remove.deletePosition(positionId)
  }

  const handleOpenCreateModal = () => {
    if (!canManage) return

    setCode('')
    setName('')
    setDescription('')
    setFormValidationError(null)
    openCreateModalState()
  }

  const handleOpenEditModal = (positionId: string) => {
    if (!canManage) return

    const position = positions.find((item) => item.id === positionId)

    if (position) {
      setCode(position.code)
      setName(position.name)
      setDescription(position.description ?? '')
      setFormValidationError(null)
    }

    openEditModalState(positionId)
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const parsed = positionFormSchema.safeParse({
      code,
      name,
      description,
    })

    if (!parsed.success) {
      setFormValidationError(parsed.error.issues[0]?.message ?? 'Dados inválidos para posição.')
      return
    }

    setFormValidationError(null)

    const payload = {
      code: parsed.data.code,
      name: parsed.data.name,
      description: parsed.data.description && parsed.data.description.length > 0
        ? parsed.data.description
        : null,
    }

    if (modalMode === 'create') {
      create.createPosition(payload, {
        onSuccess: () => {
          closePositionModal()
        },
      })
      return
    }

    if (!selectedPositionId) return

    update.updatePosition(
      { id: selectedPositionId, payload },
      {
        onSuccess: () => {
          closePositionModal()
        },
      },
    )
  }

  if (!canManage) {
    return (
      <main className="max-w-3xl mx-auto px-4 py-6">
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-amber-900">
          Somente admin da associação pode cadastrar e alterar posições.
        </div>
      </main>
    )
  }

  if (isLoading) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-on-surface-variant">Carregando posições...</p>
      </div>
    )
  }

  if (isError) {
    const forbidden = apiErrorCode === ERROR_CODES.FORBIDDEN

    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-error">
          {forbidden
            ? 'Posições indisponível no seu perfil de acesso.'
            : 'Não foi possível carregar posições.'}
        </p>
      </div>
    )
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-on-surface">Posições</h1>
          <p className="text-sm text-on-surface-variant">Gestão de posições da associação</p>
        </div>

        <button
          type="button"
          onClick={handleOpenCreateModal}
          disabled={isSubmitting || isDeleting}
          className="h-10 px-4 rounded-lg border border-primary bg-primary text-white text-sm"
        >
          Nova posição
        </button>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <label htmlFor="positions-search" className="sr-only">
          Buscar posição
        </label>
        <input
          id="positions-search"
          type="text"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Buscar por código, nome ou descrição"
          className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
        />
      </section>

      <PositionList
        positions={filteredPositions}
        isDeleting={isDeleting}
        onEdit={handleOpenEditModal}
        onDelete={handleDelete}
      />

      {isPositionModalOpen ? (
        <div className="fixed inset-0 bg-black/40 p-4 flex items-center justify-center z-50">
          <div className="w-full max-w-xl bg-white rounded-xl shadow-xl border border-outline-variant p-6">
            <PositionForm
              mode={modalMode}
              code={code}
              name={name}
              description={description}
              isSubmitting={isSubmitting}
              validationError={formValidationError}
              apiErrorMessage={resolveErrorMessage(mutationErrorCode)}
              onCodeChange={setCode}
              onNameChange={setName}
              onDescriptionChange={setDescription}
              onSubmit={handleSubmit}
              onCancel={closePositionModal}
            />
          </div>
        </div>
      ) : null}

      {modalMode === 'edit' && !selectedPosition && isPositionModalOpen ? (
        <p className="text-sm text-error">Posição selecionada não encontrada para edição.</p>
      ) : null}
    </div>
  )
}