import { FormEvent, useState } from 'react'
import { useNavigate } from '@tanstack/react-router'
import { useCreatePlayer } from '@/features/players/hooks'
import { useAuthStore } from '@/features/auth/store/authStore'

export function CompletePlayerProfilePage() {
  const navigate = useNavigate()
  const currentUser = useAuthStore((s) => s.currentUser)
  const setPlayerOnboardingRequired = useAuthStore((s) => s.setPlayerOnboardingRequired)

  const createPlayer = useCreatePlayer()

  const [name, setName] = useState('')
  const [phone, setPhone] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  const submitting = createPlayer.isPending

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!currentUser?.id) {
      setValidationError('USER_NOT_FOUND')
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
        onSuccess: () => {
          setPlayerOnboardingRequired(false)
          void navigate({ to: '/' })
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

        {validationError ? <p className="text-sm text-red-600">{validationError}</p> : null}

        {createPlayer.errorCode ? (
          <p className="text-sm text-red-600">{createPlayer.errorCode}</p>
        ) : null}

        <button type="submit" className="w-full h-10 rounded bg-black text-white" disabled={submitting}>
          {submitting ? 'Salvando...' : 'Concluir cadastro'}
        </button>
      </form>
    </main>
  )
}
