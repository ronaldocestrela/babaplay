import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { FormEvent } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { MatchForm } from '../MatchForm'

describe('MatchForm', () => {
  it('deve chamar submit e cancel', async () => {
    const onSubmit = vi.fn((event: FormEvent<HTMLFormElement>) => event.preventDefault())
    const onCancel = vi.fn()

    render(
      <MatchForm
        mode="create"
        values={{ gameDayId: '', homeTeamId: '', awayTeamId: '', description: '' }}
        gameDays={[]}
        teams={[]}
        isSubmitting={false}
        validationError={null}
        apiErrorMessage={null}
        onValueChange={vi.fn()}
        onSubmit={onSubmit}
        onCancel={onCancel}
      />,
    )

    await userEvent.click(screen.getByRole('button', { name: /cancelar/i }))
    await userEvent.click(screen.getByRole('button', { name: /salvar/i }))

    expect(onCancel).toHaveBeenCalledTimes(1)
    expect(onSubmit).toHaveBeenCalledTimes(1)
  })

  it('deve renderizar mensagens de erro', () => {
    render(
      <MatchForm
        mode="edit"
        values={{ gameDayId: 'gameday-1', homeTeamId: 'team-1', awayTeamId: 'team-2', description: '' }}
        gameDays={[]}
        teams={[]}
        isSubmitting={false}
        validationError="Erro de validação"
        apiErrorMessage="Erro da API"
        onValueChange={vi.fn()}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByText('Erro de validação')).toBeInTheDocument()
    expect(screen.getByText('Erro da API')).toBeInTheDocument()
  })
})
