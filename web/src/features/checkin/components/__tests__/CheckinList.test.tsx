import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { CheckinList } from '../CheckinList'

const mockList = [
  {
    id: 'checkin-1',
    tenantId: 'tenant-1',
    playerId: 'player-1',
    gameDayId: 'gameday-1',
    checkedInAtUtc: '2026-05-05T10:00:00.000Z',
    latitude: -23.55,
    longitude: -46.63,
    distanceFromAssociationMeters: 30,
    isActive: true,
    createdAt: '2026-05-05T10:00:00.000Z',
    cancelledAtUtc: null,
  },
]

describe('CheckinList', () => {
  it('deve renderizar lista e cancelar item', async () => {
    const onCancel = vi.fn()

    render(
      <CheckinList
        list={mockList}
        filter="active"
        isLoading={false}
        isCancelling={false}
        loadingMessage="Carregando..."
        onFilterChange={vi.fn()}
        onViewByGameDay={vi.fn()}
        onCancel={onCancel}
      />,
    )

    expect(screen.getByText(/player: player-1/i)).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: /cancelar/i }))

    expect(onCancel).toHaveBeenCalledWith('checkin-1', 'player-1', 'gameday-1')
  })

  it('deve exibir estado vazio', () => {
    render(
      <CheckinList
        list={[]}
        filter="active"
        isLoading={false}
        isCancelling={false}
        loadingMessage="Carregando..."
        onFilterChange={vi.fn()}
        onViewByGameDay={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByText(/nenhum check-in encontrado/i)).toBeInTheDocument()
  })
})
