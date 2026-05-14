import { FormEvent, useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { useCreatePlayer, usePositions, useUpdatePlayerPositions } from '@/features/players/hooks'
import { useAuthStore } from '@/features/auth/store/authStore'

const ERROR_MESSAGES: Record<string, string> = {
  REQUIRED_FIELDS_MISSING: 'Preencha os campos obrigatórios para continuar.',
  [ERROR_CODES.USER_NOT_FOUND]: 'Usuário não encontrado.',
  [ERROR_CODES.POSITIONS_LIMIT_EXCEEDED]: 'Selecione no máximo 3 posições.',
  [ERROR_CODES.POSITION_NOT_FOUND]: 'Uma ou mais posições não foram encontradas.',
  [ERROR_CODES.DUPLICATE_POSITIONS]: 'Posições duplicadas não são permitidas.',
  [ERROR_CODES.INVALID_POSITION_ID]: 'Uma ou mais posições são inválidas.',
}

export function CompletePlayerProfilePage() {
  const navigate = useNavigate()
  const currentUser = useAuthStore((s) => s.currentUser)
  const setPlayerOnboardingRequired = useAuthStore((s) => s.setPlayerOnboardingRequired)

  const createPlayer = useCreatePlayer()
  const updatePlayerPositions = useUpdatePlayerPositions()
  const { data: positions = [] } = usePositions()

  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [positionIds, setPositionIds] = useState<string[]>([])
  const [validationError, setValidationError] = useState<string | null>(null)

  const submitting = createPlayer.isPending || updatePlayerPositions.isPending

  const createErrorCode = createPlayer.errorCode
  const updatePositionsErrorCode = getErrorCode(updatePlayerPositions.error)

  const togglePosition = (positionId: string) => {
    setValidationError(null)

    setPositionIds((current) => {
      if (current.includes(positionId)) {
        return current.filter((id) => id !== positionId)
      }

      if (current.length >= 3) {
        setValidationError(ERROR_CODES.POSITIONS_LIMIT_EXCEEDED)
        return current
      }

      return [...current, positionId]
    })
  }

  const completeOnboarding = () => {
    setPlayerOnboardingRequired(false)
    void navigate({ to: '/' })
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setValidationError(null)

    if (!currentUser?.id) {
      setValidationError(ERROR_CODES.USER_NOT_FOUND)
      return
    }

    if (!name.trim() || !phone.trim() || !dateOfBirth.trim()) {
      setValidationError('REQUIRED_FIELDS_MISSING')
      return
    }

    createPlayer.createPlayer(
      {
        userId: currentUser.id,
        name: name.trim(),
        phone: phone.trim(),
        dateOfBirth: dateOfBirth.trim(),
      },
      {
        onSuccess: (created) => {
          if (positionIds.length > 0) {
            updatePlayerPositions.updatePlayerPositions(
              {
                id: created.id,
                payload: {
                  positionIds,
                },
              },
              {
                onSuccess: () => {
                  completeOnboarding()
                },
              },
            )
            return
          }

          completeOnboarding()
        },
      },
    )
  }

  return (
    <main className="min-h-screen grid place-items-center p-4">
      <form onSubmit={handleSubmit} className="w-full max-w-md space-y-4 bg-white border rounded-xl p-6">
        <h1 className="text-xl font-semibold">Complete seu cadastro de jogador</h1>
        <p className="text-sm text-gray-600">
          Para acessar o sistema, informe seus dados de jogador.
        </p>

        <div>
          <label htmlFor="complete-player-name" className="block text-sm mb-1">Nome</label>
          <input
            id="complete-player-name"
            className="w-full h-10 px-3 border rounded"
            type="text"
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
          />
        </div>

        <div>
          <label htmlFor="complete-player-phone" className="block text-sm mb-1">Telefone</label>
          <input
            id="complete-player-phone"
            className="w-full h-10 px-3 border rounded"
            type="text"
            value={phone}
            onChange={(event) => setPhone(event.target.value)}
            required
          />
        </div>

        <div>
          <label htmlFor="complete-player-dob" className="block text-sm mb-1">Data de nascimento</label>
          <input
            id="complete-player-dob"
            className="w-full h-10 px-3 border rounded"
            type="date"
            value={dateOfBirth}
            onChange={(event) => setDateOfBirth(event.target.value)}
            required
          />
        </div>

        <div className="space-y-2">
          <p className="text-sm">Posições (opcional, máximo 3)</p>
          {positions.length === 0 ? (
            <p className="text-xs text-gray-600">Nenhuma posição ativa cadastrada no momento.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              {positions.map((position) => {
                const checked = positionIds.includes(position.id)

                return (
                  <label key={position.id} className="flex items-center gap-2 text-sm text-gray-700">
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => togglePosition(position.id)}
                    />
                    {position.code} - {position.name}
                  </label>
                )
              })}
            </div>
          )}
        </div>

        {validationError ? (
          <p className="text-sm text-red-600">{ERROR_MESSAGES[validationError] ?? validationError}</p>
        ) : null}

        {createPlayer.errorCode ? (
          <p className="text-sm text-red-600">{ERROR_MESSAGES[createErrorCode] ?? createErrorCode}</p>
        ) : null}

        {updatePositionsErrorCode ? (
          <p className="text-sm text-red-600">
            {ERROR_MESSAGES[updatePositionsErrorCode] ?? updatePositionsErrorCode}
          </p>
        ) : null}

        <button type="submit" className="w-full h-10 rounded bg-black text-white" disabled={submitting}>
          {submitting ? 'Salvando...' : 'Concluir cadastro'}
        </button>
      </form>
    </main>
  )
}
