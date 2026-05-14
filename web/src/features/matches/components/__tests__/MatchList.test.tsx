import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { MatchList } from '../MatchList'

describe('MatchList', () => {
  it('deve mostrar estado vazio', () => {
    render(
      <MatchList
        matches={[]}
        teams={[]}
        isDeleting={false}
        changingStatusMatchId={null}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
        onStatusChange={vi.fn()}
      />,
    )

    expect(screen.getByText(/nenhuma partida encontrada/i)).toBeInTheDocument()
  })

  it('deve disparar ações de status, edição e exclusão', async () => {
    const onStatusChange = vi.fn()
    const onEdit = vi.fn()
    const onDelete = vi.fn()

    render(
      <MatchList
        matches={[
          {
            id: 'match-1',
            tenantId: 'tenant-1',
            gameDayId: 'gameday-1',
            homeTeamId: 'team-1',
            awayTeamId: 'team-2',
            description: 'Semi',
            status: 'Pending',
            isActive: true,
            createdAt: '2026-05-05T10:00:00.000Z',
          },
        ]}
        teams={[
          { id: 'team-1', name: 'Time Azul', isActive: true },
          { id: 'team-2', name: 'Time Laranja', isActive: true },
        ]}
        isDeleting={false}
        changingStatusMatchId={null}
        onEdit={onEdit}
        onDelete={onDelete}
        onStatusChange={onStatusChange}
      />,
    )

    await userEvent.selectOptions(screen.getByLabelText(/status da partida match-1/i), 'Scheduled')
    await userEvent.click(screen.getByRole('button', { name: /editar/i }))
    await userEvent.click(screen.getByRole('button', { name: /excluir/i }))

    expect(onStatusChange).toHaveBeenCalledWith('match-1', 'Scheduled')
    expect(onEdit).toHaveBeenCalledWith('match-1')
    expect(onDelete).toHaveBeenCalledWith('match-1')
  })
})
