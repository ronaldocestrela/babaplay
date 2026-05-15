import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PlayersPage } from '../PlayersPage'
import { usePlayerStore } from '@/features/players/store/playerStore'
import { invitationService } from '@/features/tenant-invitations/services/invitationService'

vi.mock('@/features/players/hooks', () => ({
  usePlayers: vi.fn(),
  usePositions: vi.fn(),
  useCreatePlayer: vi.fn(),
  useUpdatePlayer: vi.fn(),
  useDeletePlayer: vi.fn(),
  useUpdatePlayerPositions: vi.fn(),
}))

vi.mock('@/features/tenant-invitations/services/invitationService', () => ({
  invitationService: {
    send: vi.fn(),
  },
}))

import {
  useCreatePlayer,
  useDeletePlayer,
  usePlayers,
  usePositions,
  useUpdatePlayer,
  useUpdatePlayerPositions,
} from '@/features/players/hooks'

const createPlayer = vi.fn()
const updatePlayer = vi.fn()
const deletePlayer = vi.fn()
const updatePlayerPositions = vi.fn()

function renderPlayersPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <PlayersPage />
    </QueryClientProvider>,
  )
}

describe('PlayersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    vi.mocked(invitationService.send).mockResolvedValue({
      invitationId: 'invite-1',
      tenantId: 'tenant-1',
      tenantSlug: 'tenant-slug',
      email: 'invitee@club.com',
      expiresAtUtc: '2026-01-01T00:00:00.000Z',
    })
    usePlayerStore.setState({
      search: '',
      selectedPlayerId: null,
      modalMode: 'create',
      isModalOpen: false,
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
      ],
    } as unknown as ReturnType<typeof usePositions>)

    vi.mocked(useCreatePlayer).mockReturnValue({
      createPlayer,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useUpdatePlayer).mockReturnValue({
      updatePlayer,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })

    vi.mocked(useDeletePlayer).mockReturnValue({
      deletePlayer,
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

  it('deve exibir loading enquanto carrega', () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByText(/carregando jogadores/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem de forbidden quando sem permissão', () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: {
        response: {
          data: { title: 'FORBIDDEN' },
        },
      },
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByText(/perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve renderizar lista, filtrar e enviar convite por email', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: 'player-2',
          userId: 'user-2',
          name: 'Carlos Lima',
          nickname: null,
          phone: null,
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByText('Joao Silva')).toBeInTheDocument()
    expect(screen.getByText('Carlos Lima')).toBeInTheDocument()

    await userEvent.type(screen.getByPlaceholderText(/buscar por nome/i), 'joao')

    expect(screen.getByText('Joao Silva')).toBeInTheDocument()
    expect(screen.queryByText('Carlos Lima')).not.toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: /enviar convite por e-mail/i }))
    expect(screen.getByRole('heading', { name: /enviar convite por e-mail/i })).toBeInTheDocument()

    await userEvent.type(screen.getByLabelText(/e-mail do convidado/i), 'invitee@club.com')
    await userEvent.click(screen.getByRole('button', { name: /^enviar convite$/i }))

    await waitFor(() => {
      expect(invitationService.send).toHaveBeenCalled()
      expect(vi.mocked(invitationService.send).mock.calls[0]?.[0]).toBe('invitee@club.com')
    })
  })

  it('deve abrir edição e permitir exclusão', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          positionIds: ['11111111-1111-1111-1111-111111111111'],
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)

    renderPlayersPage()

    await userEvent.click(screen.getByRole('button', { name: /editar/i }))
    expect(screen.getByRole('heading', { name: /editar jogador/i })).toBeInTheDocument()

    await waitFor(() => {
      expect(screen.getByDisplayValue('Joao Silva')).toBeInTheDocument()
      expect(screen.getByRole('checkbox')).toBeChecked()
    })

    await userEvent.click(screen.getByRole('button', { name: /excluir/i }))
    expect(deletePlayer).toHaveBeenCalledWith('player-1')

    confirmSpy.mockRestore()
  })

  it('deve exibir coluna de posições na listagem', () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          positionIds: ['11111111-1111-1111-1111-111111111111'],
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
        {
          id: 'player-2',
          userId: 'user-2',
          name: 'Carlos Lima',
          nickname: null,
          phone: null,
          dateOfBirth: null,
          positionIds: [],
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByRole('columnheader', { name: 'Posições' })).toBeInTheDocument()
    expect(screen.getByText('Goleiro')).toBeInTheDocument()
    expect(screen.getByText('Sem posição')).toBeInTheDocument()
  })

  it('deve exibir estado de salvamento no modal', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    vi.mocked(useCreatePlayer).mockReturnValue({
      createPlayer,
      isPending: true,
      isError: false,
      error: null,
      errorCode: null,
    })

    usePlayerStore.getState().openCreateModal()

    renderPlayersPage()

    expect(screen.getByRole('button', { name: /salvando/i })).toBeDisabled()
  })

  it('deve exibir mensagem para POSITION_NOT_FOUND no modal', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    vi.mocked(useUpdatePlayerPositions).mockReturnValue({
      updatePlayerPositions,
      isPending: false,
      isError: true,
      error: null,
      errorCode: 'POSITION_NOT_FOUND',
    })

    usePlayerStore.getState().openCreateModal()

    renderPlayersPage()
    expect(screen.getByText(/não foram encontradas/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem para DUPLICATE_POSITIONS no modal', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    vi.mocked(useUpdatePlayerPositions).mockReturnValue({
      updatePlayerPositions,
      isPending: false,
      isError: true,
      error: null,
      errorCode: 'DUPLICATE_POSITIONS',
    })

    usePlayerStore.getState().openCreateModal()

    renderPlayersPage()
    expect(screen.getByText(/duplicadas não são permitidas/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem para POSITIONS_LIMIT_EXCEEDED no modal', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    vi.mocked(useUpdatePlayerPositions).mockReturnValue({
      updatePlayerPositions,
      isPending: false,
      isError: true,
      error: null,
      errorCode: 'POSITIONS_LIMIT_EXCEEDED',
    })

    usePlayerStore.getState().openCreateModal()

    renderPlayersPage()
    expect(screen.getByText(/máximo de 3 posições/i)).toBeInTheDocument()
  })

  it('deve exibir mensagem genérica quando falha sem forbidden', () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: {
        response: {
          data: { title: 'UNKNOWN_ERROR' },
        },
      },
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByText(/não foi possível carregar jogadores/i)).toBeInTheDocument()
  })

  it('não deve excluir jogador quando confirmação é cancelada', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false)

    renderPlayersPage()

    await userEvent.click(screen.getByRole('button', { name: /excluir/i }))

    expect(deletePlayer).not.toHaveBeenCalled()
    confirmSpy.mockRestore()
  })

  it('deve impedir envio de convite com e-mail inválido', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    await userEvent.click(screen.getByRole('button', { name: /enviar convite por e-mail/i }))
    await userEvent.type(screen.getByLabelText(/e-mail do convidado/i), 'email-invalido')

    expect(screen.getByText(/informe um e-mail válido/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /^enviar convite$/i })).toBeDisabled()
  })

  it('não deve atualizar jogador ao salvar em modo edição sem seleção válida', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    usePlayerStore.setState({
      isModalOpen: true,
      modalMode: 'edit',
      selectedPlayerId: 'player-not-found',
    })

    renderPlayersPage()

    await userEvent.type(screen.getByLabelText(/^nome$/i), 'Nome Atualizado')
    await userEvent.click(screen.getByRole('button', { name: /^salvar$/i }))

    expect(updatePlayer).not.toHaveBeenCalled()
  })

  it('deve exibir lista vazia quando filtro não encontra jogadores', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: 'JS10',
          phone: '11999990001',
          dateOfBirth: null,
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    await userEvent.type(screen.getByPlaceholderText(/buscar por nome/i), 'nao-encontra')

    expect(screen.getByText(/nenhum jogador encontrado/i)).toBeInTheDocument()
  })

  it('deve exibir fallback quando posição do jogador não existe no catálogo', () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [
        {
          id: 'player-1',
          userId: 'user-1',
          name: 'Joao Silva',
          nickname: null,
          phone: null,
          dateOfBirth: null,
          positionIds: ['position-not-found'],
          isActive: true,
          createdAt: '2026-01-01T00:00:00.000Z',
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    renderPlayersPage()

    expect(screen.getByText(/posição não encontrada/i)).toBeInTheDocument()
  })

  it('deve bloquear criação quando userId não é informado', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    usePlayerStore.getState().openCreateModal()
    renderPlayersPage()

    await userEvent.type(screen.getByLabelText(/^nome$/i), 'Novo Jogador')
    await userEvent.click(screen.getByRole('button', { name: /^salvar$/i }))

    expect(createPlayer).not.toHaveBeenCalled()
  })

  it('deve exibir fallback de erro ao enviar convite quando código é desconhecido', async () => {
    vi.mocked(usePlayers).mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof usePlayers>)

    vi.mocked(invitationService.send).mockRejectedValue({
      response: {
        data: { title: 'ANY_UNKNOWN_CODE' },
      },
    })

    renderPlayersPage()

    await userEvent.click(screen.getByRole('button', { name: /enviar convite por e-mail/i }))
    await userEvent.type(screen.getByLabelText(/e-mail do convidado/i), 'invitee@club.com')
    await userEvent.click(screen.getByRole('button', { name: /^enviar convite$/i }))

    await waitFor(() => {
      expect(screen.getAllByText(/não foi possível enviar o convite/i).length).toBeGreaterThan(0)
    })
  })

})