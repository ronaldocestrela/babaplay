import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { AuthHeader } from '../AuthHeader'
import { useAuthStore } from '@/features/auth/store/authStore'
import { createWrapper } from '@/test/utils'
import { mockAuthResponse, mockUserProfile } from '@/test/handlers'

const mockNavigate = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

function renderHeader() {
  return render(<AuthHeader />, { wrapper: createWrapper() })
}

describe('AuthHeader', () => {
  beforeEach(() => {
    useAuthStore.getState().clearTokens()
    mockNavigate.mockClear()
  })

  it('deve exibir logo/nome da aplicação', () => {
    renderHeader()
    expect(screen.getByText(/babaplay/i)).toBeInTheDocument()
  })

  it('deve exibir email do usuário quando disponível no store', () => {
    useAuthStore.getState().setTokens(mockAuthResponse)
    useAuthStore.getState().setCurrentUser(mockUserProfile)
    renderHeader()
    expect(screen.getByText(mockUserProfile.email)).toBeInTheDocument()
  })

  it('deve exibir botão de sair', () => {
    renderHeader()
    expect(screen.getByRole('button', { name: /sair/i })).toBeInTheDocument()
  })

  it('deve exibir ações de navegação do header', () => {
    renderHeader()

    expect(screen.getByRole('button', { name: /dashboard/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /jogadores/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /check-ins/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /times/i })).toBeInTheDocument()
  })

  it('deve navegar para /players ao clicar em jogadores', async () => {
    renderHeader()

    await userEvent.click(screen.getByRole('button', { name: /jogadores/i }))

    expect(mockNavigate).toHaveBeenCalledWith({ to: '/players' })
  })

  it('deve navegar para /checkins ao clicar em check-ins', async () => {
    renderHeader()

    await userEvent.click(screen.getByRole('button', { name: /check-ins/i }))

    expect(mockNavigate).toHaveBeenCalledWith({ to: '/checkins' })
  })

  it('deve navegar para /teams ao clicar em times', async () => {
    renderHeader()

    await userEvent.click(screen.getByRole('button', { name: /times/i }))

    expect(mockNavigate).toHaveBeenCalledWith({ to: '/teams' })
  })

  it('deve limpar sessão e navegar para /login ao clicar em sair', async () => {
    useAuthStore.getState().setTokens(mockAuthResponse)
    useAuthStore.getState().setCurrentUser(mockUserProfile)
    renderHeader()

    await userEvent.click(screen.getByRole('button', { name: /sair/i }))

    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(false)
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/login' })
    })
  })
})
