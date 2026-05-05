import type { TeamPlayerOption } from '../types'

interface TeamRosterEditorProps {
  teamName: string
  maxPlayers: number
  players: TeamPlayerOption[]
  selectedPlayerIds: string[]
  isSubmitting: boolean
  errorMessage: string | null
  onTogglePlayer: (playerId: string) => void
  onSave: () => void
  onCancel: () => void
}

export function TeamRosterEditor({
  teamName,
  maxPlayers,
  players,
  selectedPlayerIds,
  isSubmitting,
  errorMessage,
  onTogglePlayer,
  onSave,
  onCancel,
}: TeamRosterEditorProps) {
  return (
    <section className="space-y-4">
      <h2 className="text-lg font-medium text-on-surface">Elenco: {teamName}</h2>

      <p className="text-sm text-on-surface-variant">
        Selecionados: {selectedPlayerIds.length} de {maxPlayers}
      </p>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-72 overflow-auto rounded-lg border border-outline-variant p-3">
        {players.map((player) => (
          <label key={player.id} className="flex items-center gap-2 text-sm text-on-surface-variant">
            <input
              type="checkbox"
              checked={selectedPlayerIds.includes(player.id)}
              onChange={() => onTogglePlayer(player.id)}
              disabled={isSubmitting}
            />
            {player.name}
          </label>
        ))}
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
          type="button"
          onClick={onSave}
          disabled={isSubmitting}
          className="h-10 px-4 rounded-lg border border-primary bg-primary text-white disabled:opacity-60"
        >
          {isSubmitting ? 'Salvando...' : 'Salvar elenco'}
        </button>
      </div>

      {errorMessage ? <p className="text-sm text-error">{errorMessage}</p> : null}
    </section>
  )
}
