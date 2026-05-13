import type { Checkin } from '../types'

interface CheckinListProps {
  list: Checkin[]
  isLoading: boolean
  loadingMessage: string
}

export function CheckinList({
  list,
  isLoading,
  loadingMessage,
}: CheckinListProps) {
  return (
    <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 space-y-4">
      <h2 className="text-lg font-medium text-on-surface">Meus check-ins</h2>

      {isLoading ? <p className="text-sm text-on-surface-variant">{loadingMessage}</p> : null}

      {!isLoading && list.length === 0 ? (
        <p className="text-sm text-on-surface-variant">Nenhum check-in encontrado para o seu perfil.</p>
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
                <p className="text-on-surface-variant">
                  Status: {checkin.isActive ? 'Ativo' : 'Cancelado'}
                </p>
              </div>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  )
}
