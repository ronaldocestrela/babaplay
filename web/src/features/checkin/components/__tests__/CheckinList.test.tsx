import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
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
  it('deve renderizar lista de check-ins', () => {
    render(
      <CheckinList
        list={mockList}
        isLoading={false}
        loadingMessage="Carregando..."
      />,
    )

    expect(screen.getByRole('heading', { name: /meus check-ins/i })).toBeInTheDocument()
    expect(screen.getByText(/player: player-1/i)).toBeInTheDocument()
  })

  it('deve exibir estado vazio', () => {
    render(
      <CheckinList
        list={[]}
        isLoading={false}
        loadingMessage="Carregando..."
      />,
    )

    expect(screen.getByText(/nenhum check-in encontrado/i)).toBeInTheDocument()
  })

  it('deve exibir loading quando consulta estiver pendente', () => {
    render(
      <CheckinList
        list={mockList}
        isLoading={true}
        loadingMessage="Carregando..."
      />,
    )

    expect(screen.getByText(/carregando/i)).toBeInTheDocument()
  })
})
