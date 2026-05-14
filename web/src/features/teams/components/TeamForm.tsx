import type { FormEvent } from 'react'

interface TeamFormProps {
  mode: 'create' | 'edit'
  name: string
  maxPlayers: number
  isSubmitting: boolean
  validationError: string | null
  apiErrorMessage: string | null
  onNameChange: (value: string) => void
  onMaxPlayersChange: (value: number) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onCancel: () => void
}

export function TeamForm({
  mode,
  name,
  maxPlayers,
  isSubmitting,
  validationError,
  apiErrorMessage,
  onNameChange,
  onMaxPlayersChange,
  onSubmit,
  onCancel,
}: TeamFormProps) {
  return (
    <section className="space-y-4">
      <h2 className="text-lg font-medium text-on-surface">
        {mode === 'create' ? 'Novo time' : 'Editar time'}
      </h2>

      <form className="space-y-4" onSubmit={onSubmit} noValidate>
        <div className="space-y-1">
          <label htmlFor="team-name" className="text-sm text-on-surface">
            Nome
          </label>
          <input
            id="team-name"
            type="text"
            value={name}
            onChange={(event) => onNameChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="team-max-players" className="text-sm text-on-surface">
            Máximo de jogadores
          </label>
          <input
            id="team-max-players"
            type="number"
            min={1}
            value={maxPlayers}
            onChange={(event) => onMaxPlayersChange(Number(event.target.value))}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          />
        </div>

        <div className="flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="h-10 px-4 rounded-lg border border-outline-variant text-on-surface"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="h-10 px-4 rounded-lg border border-primary bg-primary text-white disabled:opacity-60"
          >
            {isSubmitting ? 'Salvando...' : 'Salvar'}
          </button>
        </div>
      </form>

      {validationError ? <p className="text-sm text-error">{validationError}</p> : null}
      {apiErrorMessage ? <p className="text-sm text-error">{apiErrorMessage}</p> : null}
    </section>
  )
}
