import type { FormEvent } from 'react'

interface PositionFormProps {
  mode: 'create' | 'edit'
  code: string
  name: string
  description: string
  isSubmitting: boolean
  validationError: string | null
  apiErrorMessage: string | null
  onCodeChange: (value: string) => void
  onNameChange: (value: string) => void
  onDescriptionChange: (value: string) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onCancel: () => void
}

export function PositionForm({
  mode,
  code,
  name,
  description,
  isSubmitting,
  validationError,
  apiErrorMessage,
  onCodeChange,
  onNameChange,
  onDescriptionChange,
  onSubmit,
  onCancel,
}: PositionFormProps) {
  return (
    <section className="space-y-4">
      <h2 className="text-lg font-medium text-on-surface">
        {mode === 'create' ? 'Nova posição' : 'Editar posição'}
      </h2>

      <form className="space-y-4" onSubmit={onSubmit} noValidate>
        <div className="space-y-1">
          <label htmlFor="position-code" className="text-sm text-on-surface">
            Código
          </label>
          <input
            id="position-code"
            type="text"
            value={code}
            onChange={(event) => onCodeChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="position-name" className="text-sm text-on-surface">
            Nome
          </label>
          <input
            id="position-name"
            type="text"
            value={name}
            onChange={(event) => onNameChange(event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="position-description" className="text-sm text-on-surface">
            Descrição
          </label>
          <textarea
            id="position-description"
            value={description}
            onChange={(event) => onDescriptionChange(event.target.value)}
            className="w-full min-h-24 px-3 py-2 rounded-lg border border-outline-variant bg-surface"
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