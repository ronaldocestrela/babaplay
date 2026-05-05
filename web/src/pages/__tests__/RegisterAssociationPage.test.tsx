import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { RegisterAssociationPage } from '../RegisterAssociationPage'

const mockNavigate = vi.fn()
const createAssociation = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/tenant-onboarding/hooks', () => ({
  useCreateAssociation: vi.fn(),
}))

import { useCreateAssociation } from '@/features/tenant-onboarding/hooks'

describe('RegisterAssociationPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    createAssociation.mockImplementation((payload, options) => {
      options?.onSuccess?.({
        id: 'tenant-123',
        name: payload.name,
        slug: payload.slug,
        provisioningStatus: 'Pending',
      })
    })

    vi.mocked(useCreateAssociation).mockReturnValue({
      createAssociation,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve renderizar formulário de cadastro de associação', () => {
    render(<RegisterAssociationPage />)

    expect(screen.getByRole('heading', { name: /registrar nova associação/i })).toBeInTheDocument()
    expect(screen.getByLabelText(/nome da associação/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^slug$/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /criar associação/i })).toBeInTheDocument()
  })

  it('deve enviar payload válido e navegar para página de status', async () => {
    render(<RegisterAssociationPage />)

    await userEvent.type(screen.getByLabelText(/nome da associação/i), 'Associação Atlética')
    await userEvent.type(screen.getByLabelText(/^slug$/i), 'associacao-atletica')
    await userEvent.click(screen.getByRole('button', { name: /criar associação/i }))

    await waitFor(() => {
      expect(createAssociation).toHaveBeenCalledWith(
        { name: 'Associação Atlética', slug: 'associacao-atletica' },
        expect.any(Object),
      )
      expect(mockNavigate).toHaveBeenCalledWith({
        to: '/register-association/status/$tenantId',
        params: { tenantId: 'tenant-123' },
      })
    })
  })

  it('deve exibir erro de validação para slug inválido', async () => {
    render(<RegisterAssociationPage />)

    await userEvent.type(screen.getByLabelText(/nome da associação/i), 'Associação')
    await userEvent.type(screen.getByLabelText(/^slug$/i), 'Slug Inválido')
    await userEvent.click(screen.getByRole('button', { name: /criar associação/i }))

    await waitFor(() => {
      expect(screen.getByText(/slug deve conter apenas letras minúsculas, números e hífens/i)).toBeInTheDocument()
    })
  })
})
