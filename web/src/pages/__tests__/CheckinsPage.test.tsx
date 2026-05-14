import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CheckinsPage } from '../CheckinsPage'
import { useAuthStore } from '@/features/auth/store/authStore'
import { ERROR_CODES } from '@/core/constants/errorCodes'

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

  it('deve mostrar erro quando geolocalização não é suportada', async () => {
    Object.defineProperty(global.navigator, 'geolocation', {
      configurable: true,
      value: undefined,
    })

    render(<CheckinsPage />)

    await userEvent.click(screen.getByRole('button', { name: /usar minha localização/i }))

    expect(screen.getByText(/geolocalização não suportada/i)).toBeInTheDocument()
  })

  it('deve mostrar erro quando geolocalização falha', async () => {
    Object.defineProperty(global.navigator, 'geolocation', {
      configurable: true,
      value: {
        getCurrentPosition: (_success: unknown, error: () => void) => {
          error()
        },
      },
    })

    render(<CheckinsPage />)

    await userEvent.click(screen.getByRole('button', { name: /usar minha localização/i }))

    expect(screen.getByText(/não foi possível obter sua localização/i)).toBeInTheDocument()
  })

  it('deve exibir feedback de erro quando criação falha', async () => {
    createCheckin.mockImplementation((_payload, options) => {
      options?.onError?.({
        response: {
          data: {
            title: 'CHECKIN_OUTSIDE_ALLOWED_RADIUS',
          },
        },
      })
    })

    render(<CheckinsPage />)

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-1')
    await userEvent.type(screen.getByLabelText(/latitude/i), '-23.5505')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')
    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(screen.getByText(/fora do raio permitido/i)).toBeInTheDocument()
  })

  it('deve ocultar check-ins inativos na lista', () => {
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
        {
          id: 'checkin-2',
          tenantId: 'tenant-1',
          playerId: 'player-inactive',
          gameDayId: 'gameday-1',
          checkedInAtUtc: '2026-05-05T10:00:00.000Z',
          latitude: -23.55,
          longitude: -46.63,
          distanceFromAssociationMeters: 40,
          isActive: false,
          createdAt: '2026-05-05T10:00:00.000Z',
          cancelledAtUtc: '2026-05-05T11:00:00.000Z',
        },
      ],
      isLoading: false,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinsByPlayer>)

    render(<CheckinsPage />)

    expect(screen.getByText(/player: player-1/i)).toBeInTheDocument()
    expect(screen.queryByText(/player: player-inactive/i)).not.toBeInTheDocument()
  })

  it('deve preencher latitude e longitude ao usar geolocalização com sucesso', async () => {
    Object.defineProperty(global.navigator, 'geolocation', {
      configurable: true,
      value: {
        getCurrentPosition: (success: (value: { coords: { latitude: number; longitude: number } }) => void) => {
          success({ coords: { latitude: -23.1234, longitude: -46.9876 } })
        },
      },
    })

    render(<CheckinsPage />)

    await userEvent.click(screen.getByRole('button', { name: /usar minha localização/i }))

    expect(screen.getByLabelText(/latitude/i)).toHaveValue(-23.1234)
    expect(screen.getByLabelText(/longitude/i)).toHaveValue(-46.9876)
  })

  it('deve exibir loading da lista quando consulta de check-ins está pendente', () => {
    vi.mocked(useCheckinsByPlayer).mockReturnValue({
      data: [],
      isLoading: true,
      error: null,
      isError: false,
    } as ReturnType<typeof useCheckinsByPlayer>)

    render(<CheckinsPage />)

    expect(screen.getByText(/carregando seus check-ins/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de erro da API quando query retorna FORBIDDEN', () => {
    vi.mocked(useCheckinsByPlayer).mockReturnValue({
      data: [],
      isLoading: false,
      error: {
        response: {
          data: { title: ERROR_CODES.FORBIDDEN },
        },
      },
      isError: true,
    } as ReturnType<typeof useCheckinsByPlayer>)

    render(<CheckinsPage />)

    expect(screen.getByText(/indisponível para o seu perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve exibir fallback genérico quando criação falha sem código de erro', async () => {
    createCheckin.mockImplementation((_payload, options) => {
      options?.onError?.({})
    })

    render(<CheckinsPage />)

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-1')
    await userEvent.type(screen.getByLabelText(/latitude/i), '-23.5505')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')
    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(screen.getByText(/não foi possível concluir a ação/i)).toBeInTheDocument()
  })

  it('deve exibir erro de validação quando coordenadas são inválidas', async () => {
    render(<CheckinsPage />)

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-1')
    await userEvent.type(screen.getByLabelText(/latitude/i), '999')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')
    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(createCheckin).not.toHaveBeenCalled()
    expect(screen.getByText(/latitude inválida/i)).toBeInTheDocument()
  })
})
