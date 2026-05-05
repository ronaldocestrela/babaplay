import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TeamsPage } from '../TeamsPage'
import { useTeamStore } from '@/features/teams/store/teamStore'

vi.mock('@/features/teams/hooks', () => ({
  useTeams: vi.fn(),
  useTeam: vi.fn(),
  useTeamPlayers: vi.fn(),
  useCreateTeam: vi.fn(),
  useUpdateTeam: vi.fn(),
  useDeleteTeam: vi.fn(),
  useUpdateTeamPlayers: vi.fn(),
}))

import {
  useCreateTeam,
  useDeleteTeam,
  useTeam,
  useTeamPlayers,
  useTeams,
  useUpdateTeam,
  useUpdateTeamPlayers,
} from '@/features/teams/hooks'

const createTeam = vi.fn()
const updateTeam = vi.fn()
const deleteTeam = vi.fn()
const updateTeamPlayers = vi.fn()

describe('TeamsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    createTeam.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })
    updateTeam.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })
    updateTeamPlayers.mockImplementation((_, options) => {
      options?.onSuccess?.()
    })

    useTeamStore.setState({
      search: '',
      selectedTeamId: null,
      modalMode: 'create',
      isTeamModalOpen: false,
      isRosterModalOpen: false,
    })

    vi.mocked(useTeams).mockReturnValue({
      data: [
        {
          id: 'team-1',
          tenantId: 'tenant-1',
          name: 'Time Azul',
          maxPlayers: 8,
          isActive: true,
          createdAt: '2026-05-05T10:00:00.000Z',
          playerIds: ['player-1'],
        },
        {
          id: 'team-2',
          tenantId: 'tenant-1',
          name: 'Time Laranja',
          maxPlayers: 9,
          isActive: true,
          createdAt: '2026-05-05T10:00:00.000Z',
          playerIds: [],
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useTeams>)

    vi.mocked(useTeam).mockReturnValue({
      data: {
        id: 'team-1',
        tenantId: 'tenant-1',
        name: 'Time Azul',
        maxPlayers: 8,
        isActive: true,
        createdAt: '2026-05-05T10:00:00.000Z',
        playerIds: ['player-1'],
      },
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useTeam>)

    vi.mocked(useTeamPlayers).mockReturnValue({
      data: [
        { id: 'player-1', name: 'Joao Silva', isActive: true },
        { id: 'player-2', name: 'Carlos Lima', isActive: true },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as ReturnType<typeof useTeamPlayers>)

    vi.mocked(useCreateTeam).mockReturnValue({
      createTeam,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useUpdateTeam).mockReturnValue({
      updateTeam,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useDeleteTeam).mockReturnValue({
      deleteTeam,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useUpdateTeamPlayers).mockReturnValue({
      updateTeamPlayers,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve renderizar lista e filtro', async () => {
    render(<TeamsPage />)

    expect(screen.getByText('Time Azul')).toBeInTheDocument()
    expect(screen.getByText('Time Laranja')).toBeInTheDocument()

    await userEvent.type(screen.getByPlaceholderText(/buscar por nome/i), 'azul')

    expect(screen.getByText('Time Azul')).toBeInTheDocument()
    expect(screen.queryByText('Time Laranja')).not.toBeInTheDocument()
  })

  it('deve abrir modal de criação e criar time', async () => {
    render(<TeamsPage />)

    await userEvent.click(screen.getByRole('button', { name: /novo time/i }))
    expect(screen.getByRole('heading', { name: /novo time/i })).toBeInTheDocument()

    await userEvent.type(screen.getByLabelText(/nome/i), 'Time Roxo')
    await userEvent.clear(screen.getByLabelText(/máximo de jogadores/i))
    await userEvent.type(screen.getByLabelText(/máximo de jogadores/i), '10')
    await userEvent.click(screen.getByRole('button', { name: /^salvar$/i }))

    expect(createTeam).toHaveBeenCalledTimes(1)
  })

  it('deve abrir modal de elenco e salvar jogadores', async () => {
    render(<TeamsPage />)

    const rosterButtons = screen.getAllByRole('button', { name: /elenco/i })
    await userEvent.click(rosterButtons[0]!)
    expect(screen.getByRole('heading', { name: /elenco: time azul/i })).toBeInTheDocument()

    await userEvent.click(screen.getByLabelText(/carlos lima/i))
    await userEvent.click(screen.getByRole('button', { name: /salvar elenco/i }))

    expect(updateTeamPlayers).toHaveBeenCalledTimes(1)
  })

  it('deve exibir mensagem quando sem permissão', () => {
    vi.mocked(useTeams).mockReturnValue({
      data: [],
      isLoading: false,
      isError: true,
      error: {
        response: {
          data: { title: 'FORBIDDEN' },
        },
      },
    } as ReturnType<typeof useTeams>)

    render(<TeamsPage />)

    expect(screen.getByText(/perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve excluir time quando confirmado', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<TeamsPage />)

    const deleteButtons = screen.getAllByRole('button', { name: /excluir/i })
    await userEvent.click(deleteButtons[0]!)

    await waitFor(() => {
      expect(deleteTeam).toHaveBeenCalledWith('team-1')
    })

    confirmSpy.mockRestore()
  })
})
