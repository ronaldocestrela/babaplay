import type { Position } from '../types'

interface PositionListProps {
  positions: Position[]
  isDeleting: boolean
  onEdit: (id: string) => void
  onDelete: (id: string) => void
}

export function PositionList({ positions, isDeleting, onEdit, onDelete }: PositionListProps) {
  if (positions.length === 0) {
    return (
      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <p className="text-sm text-on-surface-variant">Nenhuma posição encontrada.</p>
      </section>
    )
  }

  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden">
      <table className="w-full text-sm">
        <thead className="bg-surface-container-high text-on-surface-variant">
          <tr>
            <th className="text-left p-3 font-medium">Código</th>
            <th className="text-left p-3 font-medium">Nome</th>
            <th className="text-left p-3 font-medium">Descrição</th>
            <th className="text-left p-3 font-medium">Status</th>
            <th className="text-right p-3 font-medium">Ações</th>
          </tr>
        </thead>
        <tbody>
          {positions.map((position) => (
            <tr key={position.id} className="border-t border-outline-variant">
              <td className="p-3 text-on-surface">{position.code}</td>
              <td className="p-3 text-on-surface">{position.name}</td>
              <td className="p-3 text-on-surface-variant">{position.description ?? '-'}</td>
              <td className="p-3 text-on-surface-variant">
                {position.isActive ? 'Ativa' : 'Inativa'}
              </td>
              <td className="p-3">
                <div className="flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => onEdit(position.id)}
                    className="px-3 py-1.5 rounded-lg border border-outline-variant text-on-surface"
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    onClick={() => onDelete(position.id)}
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