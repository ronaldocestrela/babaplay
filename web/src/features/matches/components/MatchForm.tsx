import type { FormEvent } from 'react'
import type { MatchGameDayOption, MatchTeamOption } from '../types'

interface MatchFormValues {
  gameDayId: string
  homeTeamId: string
  awayTeamId: string
  description: string
}

interface MatchFormProps {
  mode: 'create' | 'edit'
  values: MatchFormValues
  gameDays: MatchGameDayOption[]
  teams: MatchTeamOption[]
  isSubmitting: boolean
  validationError: string | null
  apiErrorMessage: string | null
  onValueChange: (field: keyof MatchFormValues, value: string) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  onCancel: () => void
}

function formatGameDayLabel(gameDay: MatchGameDayOption) {
  const date = new Date(gameDay.scheduledAt)
  const dateLabel = Number.isNaN(date.getTime())
    ? gameDay.scheduledAt
    : date.toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      })

  return gameDay.name ? `${gameDay.name} (${dateLabel})` : dateLabel
}

export function MatchForm({
  mode,
  values,
  gameDays,
  teams,
  isSubmitting,
  validationError,
  apiErrorMessage,
  onValueChange,
  onSubmit,
  onCancel,
}: MatchFormProps) {
  return (
    <section className="space-y-4">
      <h2 className="text-lg font-medium text-on-surface">
        {mode === 'create' ? 'Nova partida' : 'Editar partida'}
      </h2>

      <form className="space-y-4" onSubmit={onSubmit} noValidate>
        <div className="space-y-1">
          <label htmlFor="match-gameday" className="text-sm text-on-surface">
            Dia de jogo
          </label>
          <select
            id="match-gameday"
            value={values.gameDayId}
            onChange={(event) => onValueChange('gameDayId', event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          >
            <option value="">Selecione um dia de jogo</option>
            {gameDays.map((gameDay) => (
              <option key={gameDay.id} value={gameDay.id}>
                {formatGameDayLabel(gameDay)}
              </option>
            ))}
          </select>
        </div>

        <div className="space-y-1">
          <label htmlFor="match-home-team" className="text-sm text-on-surface">
            Time mandante
          </label>
          <select
            id="match-home-team"
            value={values.homeTeamId}
            onChange={(event) => onValueChange('homeTeamId', event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          >
            <option value="">Selecione o time mandante</option>
            {teams
              .filter((team) => team.isActive)
              .map((team) => (
                <option key={team.id} value={team.id}>
                  {team.name}
                </option>
              ))}
          </select>
        </div>

        <div className="space-y-1">
          <label htmlFor="match-away-team" className="text-sm text-on-surface">
            Time visitante
          </label>
          <select
            id="match-away-team"
            value={values.awayTeamId}
            onChange={(event) => onValueChange('awayTeamId', event.target.value)}
            className="w-full h-10 px-3 rounded-lg border border-outline-variant bg-surface"
          >
            <option value="">Selecione o time visitante</option>
            {teams
              .filter((team) => team.isActive)
              .map((team) => (
                <option key={team.id} value={team.id}>
                  {team.name}
                </option>
              ))}
          </select>
        </div>

        <div className="space-y-1">
          <label htmlFor="match-description" className="text-sm text-on-surface">
            Descrição
          </label>
          <textarea
            id="match-description"
            value={values.description}
            onChange={(event) => onValueChange('description', event.target.value)}
            rows={3}
            className="w-full px-3 py-2 rounded-lg border border-outline-variant bg-surface"
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
