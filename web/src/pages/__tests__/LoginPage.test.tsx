import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { LoginPage } from '../LoginPage'

const mockNavigate = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/auth/hooks/useLogin', () => ({
  useLogin: vi.fn(),
}))

import { useLogin } from '@/features/auth/hooks/useLogin'

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    vi.mocked(useLogin).mockReturnValue({
      login: vi.fn(),
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve navegar para registro de associação ao clicar no CTA', async () => {
    render(<LoginPage />)

    await userEvent.click(screen.getByRole('button', { name: /registrar nova associação/i }))

    expect(mockNavigate).toHaveBeenCalledWith({ to: '/register-association' })
  })
})
