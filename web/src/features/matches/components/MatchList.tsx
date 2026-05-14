import type { Match, MatchStatus, MatchTeamOption } from '../types'

interface MatchListProps {
  matches: Match[]
  teams: MatchTeamOption[]
  isDeleting: boolean
  changingStatusMatchId: string | null
  onEdit: (id: string) => void
  onDelete: (id: string) => void
  onStatusChange: (id: string, status: MatchStatus) => void
}

const MATCH_STATUSES: MatchStatus[] = [
  'Pending',
  'Scheduled',
  'InProgress',
  'Completed',
  'Cancelled',
]

function resolveTeamName(teams: MatchTeamOption[], teamId: string) {
  return teams.find((team) => team.id === teamId)?.name ?? 'Time não encontrado'
}

function formatStatus(status: MatchStatus) {
  if (status === 'InProgress') return 'Em andamento'
  if (status === 'Pending') return 'Pendente'
  if (status === 'Scheduled') return 'Agendada'
  if (status === 'Completed') return 'Concluída'
  return 'Cancelada'
}

export function MatchList({
  matches,
  teams,
  isDeleting,
  changingStatusMatchId,
  onEdit,
  onDelete,
  onStatusChange,
}: MatchListProps) {
  if (matches.length === 0) {
    return (
      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <p className="text-sm text-on-surface-variant">Nenhuma partida encontrada.</p>
      </section>
    )
  }

  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-surface-container-high text-on-surface-variant">
          <tr>
            <th className="text-left p-3 font-medium">Confronto</th>
            <th className="text-left p-3 font-medium">Status</th>
            <th className="text-left p-3 font-medium">Descrição</th>
            <th className="text-right p-3 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {matches.map((match) => {
            const homeTeam = resolveTeamName(teams, match.homeTeamId)
            const awayTeam = resolveTeamName(teams, match.awayTeamId)
            const isChanging = changingStatusMatchId === match.id

            return (
              <tr key={match.id} className="border-t border-outline-variant">
                <td className="p-3 text-on-surface">{`${homeTeam} x ${awayTeam}`}</td>
                <td className="p-3 text-on-surface-variant">
                  <select
                    aria-label={`Status da partida ${match.id}`}
                    value={match.status}
                    onChange={(event) => onStatusChange(match.id, event.target.value as MatchStatus)}
                    disabled={isChanging}
                    className="h-9 px-2 rounded-lg border border-outline-variant bg-surface"
                  >
                    {MATCH_STATUSES.map((status) => (
                      <option key={status} value={status}>
                        {formatStatus(status)}
                      </option>
                    ))}
                  </select>
                </td>
                <td className="p-3 text-on-surface-variant">{match.description ?? '-'}</td>
                <td className="p-3">
                  <div className="flex justify-end gap-2">
                    <button
                      type="button"
                      onClick={() => onEdit(match.id)}
                      className="px-3 py-1.5 rounded-lg border border-outline-variant text-on-surface"
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => onDelete(match.id)}
                      disabled={isDeleting}
                      className="px-3 py-1.5 rounded-lg border border-red-300 text-red-700"
                    >
                      {isDeleting ? 'Excluindo...' : 'Excluir'}
                    </button>
                  </div>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </section>
  )
}
