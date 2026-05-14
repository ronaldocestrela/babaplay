import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { RegisterAssociationPage } from '../RegisterAssociationPage'
import { geocodeAddress, lookupAddressByZipCode } from '@/core/services/addressLookup'

const mockNavigate = vi.fn()
const createAssociation = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/tenant-onboarding/hooks', () => ({
  useCreateAssociation: vi.fn(),
}))

vi.mock('@/core/components/AssociationLocationMap', () => ({
  AssociationLocationMap: () => <div data-testid="association-location-map" />,
}))

vi.mock('@/core/services/addressLookup', () => ({
  lookupAddressByZipCode: vi.fn().mockResolvedValue(null),
  geocodeAddress: vi.fn().mockResolvedValue(null),
}))

import { useCreateAssociation } from '@/features/tenant-onboarding/hooks'

async function fillRequiredAssociationFields() {
  const logoFile = new File(['fake-image'], 'logo.png', { type: 'image/png' })

  await userEvent.type(screen.getByLabelText(/rua/i), 'Rua das Palmeiras')
  await userEvent.type(screen.getByLabelText(/número/i), '123')
  await userEvent.type(screen.getByLabelText(/cidade/i), 'Sao Paulo')
  await userEvent.type(screen.getByLabelText(/estado/i), 'SP')
  await userEvent.type(screen.getByLabelText(/cep/i), '01000-000')
  await userEvent.type(screen.getByLabelText(/^latitude$/i), '-23.5505')
  await userEvent.type(screen.getByLabelText(/^longitude$/i), '-46.6333')
  await userEvent.upload(screen.getByLabelText(/logo da associação/i), logoFile)

  return logoFile
}

describe('RegisterAssociationPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    vi.mocked(lookupAddressByZipCode).mockResolvedValue(null)
    vi.mocked(geocodeAddress).mockResolvedValue(null)

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
          associationLatitude: -23.5505,
          associationLongitude: -46.6333,
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

  it('deve preencher endereço e coordenadas ao buscar CEP válido', async () => {
    vi.mocked(lookupAddressByZipCode).mockResolvedValue({
      street: 'Rua Vergueiro',
      neighborhood: 'Vila Mariana',
      city: 'Sao Paulo',
      state: 'SP',
    })
    vi.mocked(geocodeAddress).mockResolvedValue({ latitude: -23.5882, longitude: -46.6324 })

    render(<RegisterAssociationPage />)

    await userEvent.type(screen.getByLabelText(/cep/i), '04101-300')
    await userEvent.tab()

    await waitFor(() => {
      expect(screen.getByLabelText(/rua/i)).toHaveValue('Rua Vergueiro')
      expect(screen.getByLabelText(/bairro/i)).toHaveValue('Vila Mariana')
      expect(screen.getByLabelText(/cidade/i)).toHaveValue('Sao Paulo')
      expect(screen.getByLabelText(/estado/i)).toHaveValue('SP')
      expect(screen.getByLabelText(/^latitude$/i)).toHaveValue('-23.588200')
      expect(screen.getByLabelText(/^longitude$/i)).toHaveValue('-46.632400')
      expect(screen.getByText(/endereço preenchido automaticamente com base no cep/i)).toBeInTheDocument()
      expect(screen.getByText(/localização da associação atualizada automaticamente no mapa/i)).toBeInTheDocument()
    })
  })
})
