import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CheckinsPage } from '../CheckinsPage'
import { useAuthStore } from '@/features/auth/store/authStore'

vi.mock('@/features/checkin/hooks', () => ({
  useCheckinPlayers: vi.fn(),
  useCheckinGameDays: vi.fn(),
  useCheckinsByPlayer: vi.fn(),
  useCreateCheckin: vi.fn(),
}))

import {
  useCheckinGameDays,
  useCheckinPlayers,
  useCheckinsByPlayer,
  useCreateCheckin,
} from '@/features/checkin/hooks'

const createCheckin = vi.fn()

describe('CheckinsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    createCheckin.mockReset()

    createCheckin.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })

    useAuthStore.setState({
      currentUser: {
        id: 'user-123',
        email: 'player@example.com',
        roles: ['Player'],
        isActive: true,
        createdAt: '2026-01-01T00:00:00.000Z',
      },
    })

    vi.mocked(useCheckinPlayers).mockReturnValue({
      data: [
        { id: 'player-1', userId: 'user-123', name: 'Joao Silva', isActive: true },
        { id: 'player-2', userId: 'user-999', name: 'Carlos Lima', isActive: true },
      ],
      isLoading: false,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinPlayers>)

    vi.mocked(useCheckinGameDays).mockReturnValue({
      data: [
        { id: 'gameday-1', scheduledAt: '2026-05-05T10:00:00.000Z', status: 'Confirmed' },
      ],
      isLoading: false,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinGameDays>)

    vi.mocked(useCheckinsByPlayer).mockReturnValue({
      data: [
        {
          id: 'checkin-1',
          tenantId: 'tenant-1',
          playerId: 'player-1',
          gameDayId: 'gameday-1',
          checkedInAtUtc: '2026-05-05T10:00:00.000Z',
          latitude: -23.55,
          longitude: -46.63,
          distanceFromAssociationMeters: 30,
          isActive: true,
          createdAt: '2026-05-05T10:00:00.000Z',
          cancelledAtUtc: null,
        },
      ],
      isLoading: false,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinsByPlayer>)

    vi.mocked(useCreateCheckin).mockReturnValue({
      createCheckin,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve renderizar título da página e lista de check-ins', () => {
    render(<CheckinsPage />)

    expect(screen.getByRole('heading', { level: 1, name: /check-ins/i })).toBeInTheDocument()
    expect(screen.getByText(/player: player-1/i)).toBeInTheDocument()
  })

  it('deve exibir erro de acesso quando query retorna forbidden', () => {
    vi.mocked(useCheckinsByPlayer).mockReturnValue({
      data: [],
      isLoading: false,
      error: {
        response: {
          data: { title: 'FORBIDDEN' },
        },
      },
      isError: true,
    } as ReturnType<typeof useCheckinsByPlayer>)

    render(<CheckinsPage />)

    expect(screen.getByText(/perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve submeter criação de check-in do usuário logado com dados válidos', async () => {
    render(<CheckinsPage />)

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-1')
    await userEvent.type(screen.getByLabelText(/latitude/i), '-23.5505')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')

    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(createCheckin).toHaveBeenCalledTimes(1)
    expect(createCheckin).toHaveBeenCalledWith(
      expect.objectContaining({ playerId: 'player-1' }),
      expect.any(Object),
    )
    expect(screen.getByText(/check-in registrado com sucesso/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem quando usuário não possui perfil de jogador ativo', async () => {
    vi.mocked(useCheckinPlayers).mockReturnValue({
      data: [{ id: 'player-2', userId: 'user-999', name: 'Carlos Lima', isActive: true }],
      isLoading: false,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinPlayers>)

    render(<CheckinsPage />)

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-1')
    await userEvent.type(screen.getByLabelText(/latitude/i), '-23.5505')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')
    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(createCheckin).not.toHaveBeenCalled()
    expect(screen.getByText(/voce ainda nao possui perfil de jogador ativo/i)).toBeInTheDocument()
  })
})
