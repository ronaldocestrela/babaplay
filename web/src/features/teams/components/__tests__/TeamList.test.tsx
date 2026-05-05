import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { TeamList } from '../TeamList'

describe('TeamList', () => {
  it('deve mostrar estado vazio', () => {
    render(
      <TeamList
        teams={[]}
        isDeleting={false}
        onEdit={vi.fn()}
        onOpenRoster={vi.fn()}
        onDelete={vi.fn()}
      />,
    )

    expect(screen.getByText(/nenhum time encontrado/i)).toBeInTheDocument()
  })

  it('deve disparar ações de elenco, edição e exclusão', async () => {
    const onOpenRoster = vi.fn()
    const onEdit = vi.fn()
    const onDelete = vi.fn()

    render(
      <TeamList
        teams={[
          {
            id: 'team-1',
            tenantId: 'tenant-1',
            name: 'Time Azul',
            maxPlayers: 8,
            isActive: true,
            createdAt: '2026-05-05T10:00:00.000Z',
            playerIds: ['player-1'],
          },
        ]}
        isDeleting={false}
        onEdit={onEdit}
        onOpenRoster={onOpenRoster}
        onDelete={onDelete}
      />,
    )

    await userEvent.click(screen.getByRole('button', { name: /elenco/i }))
    await userEvent.click(screen.getByRole('button', { name: /editar/i }))
    await userEvent.click(screen.getByRole('button', { name: /excluir/i }))

    expect(onOpenRoster).toHaveBeenCalledWith('team-1')
    expect(onEdit).toHaveBeenCalledWith('team-1')
    expect(onDelete).toHaveBeenCalledWith('team-1')
  })
})
