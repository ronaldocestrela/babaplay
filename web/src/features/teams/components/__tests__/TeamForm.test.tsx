import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { FormEvent } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { TeamForm } from '../TeamForm'

describe('TeamForm', () => {
  it('deve chamar submit e cancel', async () => {
    const onSubmit = vi.fn((event: FormEvent<HTMLFormElement>) => event.preventDefault())
    const onCancel = vi.fn()

    render(
      <TeamForm
        mode="create"
        name=""
        maxPlayers={11}
        isSubmitting={false}
        validationError={null}
        apiErrorMessage={null}
        onNameChange={vi.fn()}
        onMaxPlayersChange={vi.fn()}
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
      <TeamForm
        mode="edit"
        name="Time"
        maxPlayers={9}
        isSubmitting={false}
        validationError="Erro de validação"
        apiErrorMessage="Erro da API"
        onNameChange={vi.fn()}
        onMaxPlayersChange={vi.fn()}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByText('Erro de validação')).toBeInTheDocument()
    expect(screen.getByText('Erro da API')).toBeInTheDocument()
  })
})
