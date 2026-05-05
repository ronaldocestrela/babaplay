import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { TeamRosterEditor } from '../TeamRosterEditor'

describe('TeamRosterEditor', () => {
  it('deve chamar toggle e salvar', async () => {
    const onTogglePlayer = vi.fn()
    const onSave = vi.fn()

    render(
      <TeamRosterEditor
        teamName="Time Azul"
        maxPlayers={8}
        players={[
          { id: 'player-1', name: 'Joao Silva', isActive: true },
          { id: 'player-2', name: 'Carlos Lima', isActive: true },
        ]}
        selectedPlayerIds={[]}
        isSubmitting={false}
        errorMessage={null}
        onTogglePlayer={onTogglePlayer}
        onSave={onSave}
        onCancel={vi.fn()}
      />,
    )

    await userEvent.click(screen.getByLabelText(/joao silva/i))
    await userEvent.click(screen.getByRole('button', { name: /salvar elenco/i }))

    expect(onTogglePlayer).toHaveBeenCalledWith('player-1')
    expect(onSave).toHaveBeenCalledTimes(1)
  })

  it('deve renderizar mensagem de erro', () => {
    render(
      <TeamRosterEditor
        teamName="Time Azul"
        maxPlayers={8}
        players={[]}
        selectedPlayerIds={[]}
        isSubmitting={false}
        errorMessage="Erro no elenco"
        onTogglePlayer={vi.fn()}
        onSave={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByText('Erro no elenco')).toBeInTheDocument()
  })
})
