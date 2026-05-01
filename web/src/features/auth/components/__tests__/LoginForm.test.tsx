import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi } from 'vitest'
import { LoginForm } from '../LoginForm'

describe('LoginForm', () => {
  it('deve renderizar campos de email, senha e botão de submit', () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={false} />)

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument()
  })

  it('deve exibir mensagem de erro quando email é inválido', async () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={false} />)

    await userEvent.type(screen.getByLabelText(/email/i), 'email-invalido')
    await userEvent.click(screen.getByRole('button', { name: /entrar/i }))

    await waitFor(() => {
      expect(screen.getByText(/email inválido/i)).toBeInTheDocument()
    })
  })

  it('deve exibir erro quando senha tem menos de 6 caracteres', async () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={false} />)

    await userEvent.type(screen.getByLabelText(/email/i), 'test@example.com')
    await userEvent.type(screen.getByLabelText(/senha/i), '12345')
    await userEvent.click(screen.getByRole('button', { name: /entrar/i }))

    await waitFor(() => {
      expect(screen.getByText(/no mínimo 6 caracteres/i)).toBeInTheDocument()
    })
  })

  it('deve desabilitar o botão e mostrar "Entrando..." quando isLoading=true', () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={true} />)

    const button = screen.getByRole('button', { name: /entrando/i })
    expect(button).toBeDisabled()
  })

  it('deve chamar onSubmit com os dados corretos em submit válido', async () => {
    const onSubmit = vi.fn()
    render(<LoginForm onSubmit={onSubmit} isLoading={false} />)

    await userEvent.type(screen.getByLabelText(/email/i), 'test@example.com')
    await userEvent.type(screen.getByLabelText(/senha/i), 'password123')
    await userEvent.click(screen.getByRole('button', { name: /entrar/i }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(
        { email: 'test@example.com', password: 'password123' },
        expect.anything(),
      )
    })
  })

  it('deve exibir mensagem de erro da API quando errorCode=INVALID_CREDENTIALS', () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={false} errorCode="INVALID_CREDENTIALS" />)
    expect(screen.getByText(/email ou senha inválidos/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de erro da API quando errorCode=USER_INACTIVE', () => {
    render(<LoginForm onSubmit={vi.fn()} isLoading={false} errorCode="USER_INACTIVE" />)
    expect(screen.getByText(/usuário inativo/i)).toBeInTheDocument()
  })
})
