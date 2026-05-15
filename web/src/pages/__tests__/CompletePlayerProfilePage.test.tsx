import { beforeEach, describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CompletePlayerProfilePage } from '../CompletePlayerProfilePage'
import { useAuthStore } from '@/features/auth/store/authStore'

const mockNavigate = vi.fn()
const createPlayer = vi.fn()
const updatePlayerPositions = vi.fn()

vi.mock('@tanstack/react-router', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-router')>()
  return { ...actual, useNavigate: () => mockNavigate }
})

vi.mock('@/features/players/hooks', () => ({
  useCreatePlayer: vi.fn(),
  usePositions: vi.fn(),
  useUpdatePlayerPositions: vi.fn(),
}))

import {
  useCreatePlayer,
  usePositions,
  useUpdatePlayerPositions,
} from '@/features/players/hooks'

describe('CompletePlayerProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    useAuthStore.setState({
      currentUser: {
        id: 'user-123',
        email: 'player@club.com',
        roles: ['Player'],
        isActive: true,
        createdAt: '2026-01-01T00:00:00.000Z',
        primaryTenant: null,
        tenants: [],
      },
      requiresPlayerOnboarding: true,
    })

    vi.mocked(usePositions).mockReturnValue({
      data: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          tenantId: 'tenant-1',
          code: 'GK',
          name: 'Goleiro',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: '22222222-2222-2222-2222-222222222222',
          tenantId: 'tenant-1',
          code: 'FW',
          name: 'Atacante',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
    } as unknown as ReturnType<typeof usePositions>)

    vi.mocked(useCreatePlayer).mockReturnValue({
      createPlayer,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useUpdatePlayerPositions).mockReturnValue({
      updatePlayerPositions,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve concluir onboarding com posições selecionadas', async () => {
    createPlayer.mockImplementation((_payload, options) => {
      options?.onSuccess?.({ id: 'player-1' })
    })

    updatePlayerPositions.mockImplementation((_payload, options) => {
      options?.onSuccess?.()
    })

    render(<CompletePlayerProfilePage />)

    await userEvent.type(screen.getByLabelText(/^nome$/i), 'Joao Silva')
    await userEvent.type(screen.getByLabelText(/telefone/i), '11999990001')
    await userEvent.type(screen.getByLabelText(/data de nascimento/i), '1990-01-01')
    await userEvent.click(screen.getByLabelText(/gk - goleiro/i))

    await userEvent.click(screen.getByRole('button', { name: /concluir cadastro/i }))

    await waitFor(() => {
      expect(createPlayer).toHaveBeenCalledWith(
        {
          userId: 'user-123',
          name: 'Joao Silva',
          phone: '11999990001',
          dateOfBirth: '1990-01-01',
        },
        expect.any(Object),
      )
      expect(updatePlayerPositions).toHaveBeenCalledWith(
        {
          id: 'player-1',
          payload: {
            positionIds: ['11111111-1111-1111-1111-111111111111'],
          },
        },
        expect.any(Object),
      )
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/' })
      expect(useAuthStore.getState().requiresPlayerOnboarding).toBe(false)
    })
  })

  it('deve concluir onboarding sem chamar update de posições quando nada for selecionado', async () => {
    createPlayer.mockImplementation((_payload, options) => {
      options?.onSuccess?.({ id: 'player-1' })
    })

    render(<CompletePlayerProfilePage />)

    await userEvent.type(screen.getByLabelText(/^nome$/i), 'Joao Silva')
    await userEvent.type(screen.getByLabelText(/telefone/i), '11999990001')
    await userEvent.type(screen.getByLabelText(/data de nascimento/i), '1990-01-01')

    await userEvent.click(screen.getByRole('button', { name: /concluir cadastro/i }))

    await waitFor(() => {
      expect(createPlayer).toHaveBeenCalled()
      expect(updatePlayerPositions).not.toHaveBeenCalled()
      expect(mockNavigate).toHaveBeenCalledWith({ to: '/' })
    })
  })

  it('deve bloquear seleção acima do limite de 3 posições', async () => {
    vi.mocked(usePositions).mockReturnValue({
      data: [
        {
          id: '11111111-1111-1111-1111-111111111111',
          tenantId: 'tenant-1',
          code: 'GK',
          name: 'Goleiro',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: '22222222-2222-2222-2222-222222222222',
          tenantId: 'tenant-1',
          code: 'CB',
          name: 'Zagueiro',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: '33333333-3333-3333-3333-333333333333',
          tenantId: 'tenant-1',
          code: 'CM',
          name: 'Meio-campo',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: '44444444-4444-4444-4444-444444444444',
          tenantId: 'tenant-1',
          code: 'FW',
          name: 'Atacante',
          description: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
    } as unknown as ReturnType<typeof usePositions>)

    render(<CompletePlayerProfilePage />)

    await userEvent.click(screen.getByLabelText(/gk - goleiro/i))
    await userEvent.click(screen.getByLabelText(/cb - zagueiro/i))
    await userEvent.click(screen.getByLabelText(/cm - meio-campo/i))
    await userEvent.click(screen.getByLabelText(/fw - atacante/i))

    expect(screen.getByText(/selecione no máximo 3 posições/i)).toBeInTheDocument()
  })

  it('deve exibir aviso quando não houver posições ativas', () => {
    vi.mocked(usePositions).mockReturnValue({
      data: [],
    } as unknown as ReturnType<typeof usePositions>)

    render(<CompletePlayerProfilePage />)

    expect(screen.getByText(/nenhuma posição ativa cadastrada/i)).toBeInTheDocument()
  })

  it('deve validar campos obrigatórios antes de enviar', async () => {
    render(<CompletePlayerProfilePage />)

    const submitButton = screen.getByRole('button', { name: /concluir cadastro/i })
    const form = submitButton.closest('form')
    expect(form).not.toBeNull()

    fireEvent.submit(form!)

    expect(createPlayer).not.toHaveBeenCalled()
    expect(screen.getByText(/preencha os campos obrigatórios/i)).toBeInTheDocument()
  })

  it('deve exibir erro quando usuário logado não estiver disponível', async () => {
    useAuthStore.setState({ currentUser: null })

    render(<CompletePlayerProfilePage />)

    await userEvent.type(screen.getByLabelText(/^nome$/i), 'Joao Silva')
    await userEvent.type(screen.getByLabelText(/telefone/i), '11999990001')
    await userEvent.type(screen.getByLabelText(/data de nascimento/i), '1990-01-01')
    await userEvent.click(screen.getByRole('button', { name: /concluir cadastro/i }))

    expect(createPlayer).not.toHaveBeenCalled()
    expect(screen.getByText(/usuário não encontrado/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de erro da criação quando hook retorna errorCode', () => {
    vi.mocked(useCreatePlayer).mockReturnValue({
      createPlayer,
      isPending: false,
      isError: true,
      error: null,
      errorCode: 'USER_NOT_FOUND',
    })

    render(<CompletePlayerProfilePage />)

    expect(screen.getByText(/usuário não encontrado/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de erro de atualização de posições', () => {
    vi.mocked(useUpdatePlayerPositions).mockReturnValue({
      updatePlayerPositions,
      isPending: false,
      isError: true,
      error: {
        response: {
          data: {
            title: 'POSITION_NOT_FOUND',
          },
        },
      } as unknown as Error,
      errorCode: null,
    })

    render(<CompletePlayerProfilePage />)

    expect(screen.getByText(/não foram encontradas/i)).toBeInTheDocument()
  })
})
