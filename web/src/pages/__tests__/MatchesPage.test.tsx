import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MatchesPage } from '../MatchesPage'
import { useMatchStore } from '@/features/matches/store/matchStore'

vi.mock('@/features/matches/hooks', () => ({
  useMatches: vi.fn(),
  useMatchGameDays: vi.fn(),
  useMatchTeams: vi.fn(),
  useCreateMatch: vi.fn(),
  useUpdateMatch: vi.fn(),
  useDeleteMatch: vi.fn(),
  useChangeMatchStatus: vi.fn(),
}))

import {
  useChangeMatchStatus,
  useCreateMatch,
  useDeleteMatch,
  useMatches,
  useMatchGameDays,
  useMatchTeams,
  useUpdateMatch,
} from '@/features/matches/hooks'

const createMatch = vi.fn()
const updateMatch = vi.fn()
const deleteMatch = vi.fn()
const changeMatchStatus = vi.fn()

describe('MatchesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    createMatch.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })
    updateMatch.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })
    changeMatchStatus.mockImplementation((_, options) => {
      options?.onSettled?.()
    })

    useMatchStore.setState({
      search: '',
      selectedMatchId: null,
      modalMode: 'create',
      isModalOpen: false,
    })

    vi.mocked(useMatches).mockReturnValue({
      data: [
        {
          id: 'match-1',
          tenantId: 'tenant-1',
          gameDayId: 'gameday-1',
          homeTeamId: 'team-1',
          awayTeamId: 'team-2',
          description: 'Semi',
          status: 'Pending',
          isActive: true,
          createdAt: '2026-05-05T10:00:00.000Z',
        },
        {
          id: 'match-2',
          tenantId: 'tenant-1',
          gameDayId: 'gameday-2',
          homeTeamId: 'team-2',
          awayTeamId: 'team-1',
          description: null,
          status: 'Scheduled',
          isActive: true,
          createdAt: '2026-05-05T10:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useMatches>)

    vi.mocked(useMatchGameDays).mockReturnValue({
      data: [
        { id: 'gameday-1', scheduledAt: '2026-05-10T10:00:00.000Z', status: 'Confirmed' },
        { id: 'gameday-2', scheduledAt: '2026-05-12T10:00:00.000Z', status: 'Pending' },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useMatchGameDays>)

    vi.mocked(useMatchTeams).mockReturnValue({
      data: [
        { id: 'team-1', name: 'Time Azul', isActive: true },
        { id: 'team-2', name: 'Time Laranja', isActive: true },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useMatchTeams>)

    vi.mocked(useCreateMatch).mockReturnValue({
      createMatch,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useUpdateMatch).mockReturnValue({
      updateMatch,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useDeleteMatch).mockReturnValue({
      deleteMatch,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useChangeMatchStatus).mockReturnValue({
      changeMatchStatus,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve renderizar lista e filtro', async () => {
    render(<MatchesPage />)

    expect(screen.getByText(/time azul x time laranja/i)).toBeInTheDocument()
    expect(screen.getByText(/time laranja x time azul/i)).toBeInTheDocument()

    await userEvent.type(screen.getByPlaceholderText(/buscar por time ou status/i), 'scheduled')

    expect(screen.queryByText(/time azul x time laranja/i)).not.toBeInTheDocument()
    expect(screen.getByText(/time laranja x time azul/i)).toBeInTheDocument()
  })

  it('deve abrir modal de criação e criar partida', async () => {
    render(<MatchesPage />)

    await userEvent.click(screen.getByRole('button', { name: /nova partida/i }))
    expect(screen.getByRole('heading', { name: /nova partida/i })).toBeInTheDocument()

    await userEvent.selectOptions(screen.getByLabelText(/dia de jogo/i), 'gameday-2')
    await userEvent.selectOptions(screen.getByLabelText(/time mandante/i), 'team-1')
    await userEvent.selectOptions(screen.getByLabelText(/time visitante/i), 'team-2')
    await userEvent.type(screen.getByLabelText(/descrição/i), 'Final')
    await userEvent.click(screen.getByRole('button', { name: /^salvar$/i }))

    expect(createMatch).toHaveBeenCalledTimes(1)
  })

  it('deve abrir modal de edição e atualizar partida', async () => {
    render(<MatchesPage />)

    await userEvent.click(screen.getAllByRole('button', { name: /editar/i })[0]!)
    await userEvent.clear(screen.getByLabelText(/descrição/i))
    await userEvent.type(screen.getByLabelText(/descrição/i), 'Partida editada')
    await userEvent.click(screen.getByRole('button', { name: /^salvar$/i }))

    expect(updateMatch).toHaveBeenCalledTimes(1)
  })

  it('deve alterar status da partida', async () => {
    render(<MatchesPage />)

    await userEvent.selectOptions(
      screen.getByLabelText(/status da partida match-1/i),
      'Scheduled',
    )

    await waitFor(() => {
      expect(changeMatchStatus).toHaveBeenCalledWith(
        {
          id: 'match-1',
          payload: { status: 'Scheduled' },
        },
        expect.any(Object),
      )
    })
  })

  it('deve exibir mensagem quando sem permissão', () => {
    vi.mocked(useMatches).mockReturnValue({
      data: [],
      isLoading: false,
      isError: true,
      error: {
        response: {
          data: { title: 'FORBIDDEN' },
        },
      },
    } as ReturnType<typeof useMatches>)

    render(<MatchesPage />)

    expect(screen.getByText(/perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve excluir partida quando confirmado', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<MatchesPage />)

    await userEvent.click(screen.getAllByRole('button', { name: /excluir/i })[0]!)

    await waitFor(() => {
      expect(deleteMatch).toHaveBeenCalledWith('match-1')
    })

    confirmSpy.mockRestore()
  })
})
