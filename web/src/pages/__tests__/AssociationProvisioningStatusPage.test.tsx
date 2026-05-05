import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AssociationProvisioningStatusPage } from '../AssociationProvisioningStatusPage'

const mockNavigate = vi.fn()
const mockRefetch = vi.fn()
const mockLocationAssign = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => ({ tenantId: 'tenant-123' }),
  }
})

vi.mock('@/features/tenant-onboarding/hooks', () => ({
  useAssociationStatus: vi.fn(),
}))

import { useAssociationStatus } from '@/features/tenant-onboarding/hooks'

describe('AssociationProvisioningStatusPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    Object.defineProperty(window, 'location', {
      writable: true,
      value: {
        ...window.location,
        protocol: 'https:',
        hostname: 'app.babaplay.app',
        port: '',
        assign: mockLocationAssign,
      },
    })
  })

  it('deve exibir estado de carregamento', () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: true,
      isError: false,
      isSuccess: false,
      data: undefined,
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    expect(screen.getByText(/aguardando atualização de status/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/etapas do provisionamento/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de sucesso quando status é Ready', () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Central',
        slug: 'associacao-central',
        provisioningStatus: 'Ready',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    expect(screen.getByText(/associação pronta/i)).toBeInTheDocument()
    expect(screen.getByText(/associação central/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /ir para login/i })).toBeInTheDocument()
  })

  it('deve redirecionar para login no subdomínio da associação quando status é Ready', async () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Central',
        slug: 'associacao-central',
        provisioningStatus: 'Ready',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    await userEvent.click(screen.getByRole('button', { name: /ir para login/i }))

    expect(mockLocationAssign).toHaveBeenCalledWith('https://associacao-central.babaplay.app/login')
  })

  it('deve usar fallback /login?tenant no localhost', async () => {
    Object.defineProperty(window, 'location', {
      writable: true,
      value: {
        ...window.location,
        protocol: 'http:',
        hostname: 'localhost',
        port: '5173',
        assign: mockLocationAssign,
      },
    })

    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Local',
        slug: 'associacao-local',
        provisioningStatus: 'Ready',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    await userEvent.click(screen.getByRole('button', { name: /ir para login/i }))

    expect(mockLocationAssign).toHaveBeenCalledWith('/login?tenant=associacao-local')
  })

  it('deve renderizar timeline para status Pending', () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Central',
        slug: 'associacao-central',
        provisioningStatus: 'Pending',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    expect(screen.getByLabelText(/etapas do provisionamento/i)).toBeInTheDocument()
    expect(screen.getByText(/solicitação recebida/i)).toBeInTheDocument()
    expect(screen.getByText(/ambiente pronto/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem operacional para status Failed', () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Central',
        slug: 'associacao-central',
        provisioningStatus: 'Failed',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    expect(screen.getByText(/status atual: Failed/i)).toBeInTheDocument()
    expect(
      screen.getByText(/o provisionamento falhou\. revise os dados informados ou tente criar uma nova associação\./i),
    ).toBeInTheDocument()
  })

  it('deve exibir mensagem operacional para status Cancelled', () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: false,
      isSuccess: true,
      data: {
        id: 'tenant-123',
        name: 'Associação Central',
        slug: 'associacao-central',
        provisioningStatus: 'Cancelled',
      },
      error: null,
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    expect(screen.getByText(/status atual: Cancelled/i)).toBeInTheDocument()
    expect(screen.getByText(/provisionamento foi cancelado/i)).toBeInTheDocument()
  })

  it('deve permitir retry quando consulta falha', async () => {
    vi.mocked(useAssociationStatus).mockReturnValue({
      isLoading: false,
      isError: true,
      isSuccess: false,
      data: undefined,
      error: {
        response: { data: { title: 'INTERNAL_ERROR' } },
      },
      refetch: mockRefetch,
    } as unknown as ReturnType<typeof useAssociationStatus>)

    render(<AssociationProvisioningStatusPage />)

    await userEvent.click(screen.getByRole('button', { name: /tentar novamente/i }))

    expect(mockRefetch).toHaveBeenCalled()
  })
})
