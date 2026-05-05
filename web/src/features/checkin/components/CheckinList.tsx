import type { Checkin } from '../types'

interface CheckinListProps {
  list: Checkin[]
  filter: 'active' | 'all'
  isLoading: boolean
  isCancelling: boolean
  cancellingCheckinId: string | null
  loadingMessage: string
  onFilterChange: (filter: 'active' | 'all') => void
  onViewByGameDay: () => void
  onCancel: (id: string, playerId: string, gameDayId: string) => void
}

export function CheckinList({
  list,
  filter,
  isLoading,
  isCancelling,
  cancellingCheckinId,
  loadingMessage,
  onFilterChange,
  onViewByGameDay,
  onCancel,
}: CheckinListProps) {
  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 space-y-4">
      <div className="flex flex-wrap items-center gap-3">
        <button
          type="button"
          onClick={() => onFilterChange('active')}
          className="h-9 px-3 rounded-lg border border-outline-variant"
          aria-pressed={filter === 'active'}
        >
          Somente ativos
        </button>
        <button
          type="button"
          onClick={() => onFilterChange('all')}
          className="h-9 px-3 rounded-lg border border-outline-variant"
          aria-pressed={filter === 'all'}
        >
          Todos
        </button>
        <button
          type="button"
          onClick={onViewByGameDay}
          className="h-9 px-3 rounded-lg border border-outline-variant"
        >
          Ver por game day
        </button>
      </div>

      {isLoading ? <p className="text-sm text-on-surface-variant">{loadingMessage}</p> : null}

      {!isLoading && list.length === 0 ? (
        <p className="text-sm text-on-surface-variant">Nenhum check-in encontrado para os filtros atuais.</p>
      ) : null}

      {!isLoading && list.length > 0 ? (
        <ul className="space-y-2">
          {list.map((checkin) => (
            <li
              key={checkin.id}
              className="border border-outline-variant rounded-lg p-3 flex flex-wrap items-center justify-between gap-3"
            >
              <div className="space-y-1 text-sm">
                <p className="text-on-surface">Player: {checkin.playerId}</p>
                <p className="text-on-surface-variant">GameDay: {checkin.gameDayId}</p>
                <p className="text-on-surface-variant">
                  Distância: {checkin.distanceFromAssociationMeters.toFixed(1)}m
                </p>
              </div>

              <button
                type="button"
                onClick={() => onCancel(checkin.id, checkin.playerId, checkin.gameDayId)}
                disabled={!checkin.isActive || (isCancelling && cancellingCheckinId === checkin.id)}
                className="h-9 px-3 rounded-lg border border-red-300 text-red-700 disabled:opacity-60"
              >
                {isCancelling && cancellingCheckinId === checkin.id ? 'Cancelando...' : 'Cancelar'}
              </button>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  )
}
