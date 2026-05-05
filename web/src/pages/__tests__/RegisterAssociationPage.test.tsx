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

async function fillRequiredAssociationFields() {
  const logoFile = new File(['fake-image'], 'logo.png', { type: 'image/png' })

  await userEvent.type(screen.getByLabelText(/rua/i), 'Rua das Palmeiras')
  await userEvent.type(screen.getByLabelText(/número/i), '123')
  await userEvent.type(screen.getByLabelText(/cidade/i), 'Sao Paulo')
  await userEvent.type(screen.getByLabelText(/estado/i), 'SP')
  await userEvent.type(screen.getByLabelText(/cep/i), '01000-000')
  await userEvent.upload(screen.getByLabelText(/logo da associação/i), logoFile)

  return logoFile
}

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
    expect(screen.getByLabelText(/email do admin inicial/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/senha do admin inicial/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /criar associação/i })).toBeInTheDocument()
  })

  it('deve enviar payload válido e navegar para página de status', async () => {
    render(<RegisterAssociationPage />)

    const logoFile = await fillRequiredAssociationFields()
    await userEvent.type(screen.getByLabelText(/nome da associação/i), 'Associação Atlética')
    await userEvent.type(screen.getByLabelText(/^slug$/i), 'associacao-atletica')
    await userEvent.type(screen.getByLabelText(/email do admin inicial/i), 'admin@atletica.com')
    await userEvent.type(screen.getByLabelText(/senha do admin inicial/i), 'Admin1234')
    await userEvent.type(screen.getByLabelText(/confirmar senha do admin/i), 'Admin1234')
    await userEvent.click(screen.getByRole('button', { name: /criar associação/i }))

    await waitFor(() => {
      expect(createAssociation).toHaveBeenCalledWith(
        {
          name: 'Associação Atlética',
          slug: 'associacao-atletica',
          logo: logoFile,
          street: 'Rua das Palmeiras',
          number: '123',
          neighborhood: '',
          city: 'Sao Paulo',
          state: 'SP',
          zipCode: '01000-000',
          adminEmail: 'admin@atletica.com',
          adminPassword: 'Admin1234',
        },
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

    await fillRequiredAssociationFields()
    await userEvent.type(screen.getByLabelText(/nome da associação/i), 'Associação')
    await userEvent.type(screen.getByLabelText(/^slug$/i), 'Slug Inválido')
    await userEvent.type(screen.getByLabelText(/email do admin inicial/i), 'admin@associacao.com')
    await userEvent.type(screen.getByLabelText(/senha do admin inicial/i), 'Admin1234')
    await userEvent.type(screen.getByLabelText(/confirmar senha do admin/i), 'Admin1234')
    await userEvent.click(screen.getByRole('button', { name: /criar associação/i }))

    await waitFor(() => {
      expect(screen.getByText(/slug deve conter apenas letras minúsculas, números e hífens/i)).toBeInTheDocument()
    })
  })

  it('deve validar confirmação de senha do admin', async () => {
    render(<RegisterAssociationPage />)

    await fillRequiredAssociationFields()
    await userEvent.type(screen.getByLabelText(/nome da associação/i), 'Associação')
    await userEvent.type(screen.getByLabelText(/^slug$/i), 'associacao-valida')
    await userEvent.type(screen.getByLabelText(/email do admin inicial/i), 'admin@associacao.com')
    await userEvent.type(screen.getByLabelText(/senha do admin inicial/i), 'Admin1234')
    await userEvent.type(screen.getByLabelText(/confirmar senha do admin/i), 'Admin12345')
    await userEvent.click(screen.getByRole('button', { name: /criar associação/i }))

    await waitFor(() => {
      expect(screen.getByText(/as senhas do admin devem ser iguais/i)).toBeInTheDocument()
    })
  })
})
