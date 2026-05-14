import type { Team } from '../types'

interface TeamListProps {
  teams: Team[]
  isDeleting: boolean
  onEdit: (id: string) => void
  onOpenRoster: (id: string) => void
  onDelete: (id: string) => void
}

export function TeamList({ teams, isDeleting, onEdit, onOpenRoster, onDelete }: TeamListProps) {
  if (teams.length === 0) {
    return (
      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <p className="text-sm text-on-surface-variant">Nenhum time encontrado.</p>
      </section>
    )
  }

  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-surface-container-high text-on-surface-variant">
          <tr>
            <th className="text-left p-3 font-medium">Nome</th>
            <th className="text-left p-3 font-medium">Máximo</th>
            <th className="text-left p-3 font-medium">Elenco</th>
            <th className="text-left p-3 font-medium">Status</th>
            <th className="text-right p-3 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {teams.map((team) => (
            <tr key={team.id} className="border-t border-outline-variant">
              <td className="p-3 text-on-surface">{team.name}</td>
              <td className="p-3 text-on-surface-variant">{team.maxPlayers}</td>
              <td className="p-3 text-on-surface-variant">{team.playerIds.length}</td>
              <td className="p-3 text-on-surface-variant">{team.isActive ? 'Ativo' : 'Inativo'}</td>
              <td className="p-3">
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => onOpenRoster(team.id)}
                    className="px-3 py-1.5 rounded-lg border border-outline-variant text-on-surface"
                  >
                    Elenco
                  </button>
                  <button
                    type="button"
                    onClick={() => onEdit(team.id)}
                    className="px-3 py-1.5 rounded-lg border border-outline-variant text-on-surface"
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    onClick={() => onDelete(team.id)}
                    disabled={isDeleting}
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
    </section>
  )
}
