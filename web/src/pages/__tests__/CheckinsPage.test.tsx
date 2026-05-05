import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CheckinsPage } from '../CheckinsPage'
import { useCheckinStore } from '@/features/checkin/store/checkinStore'

vi.mock('@/features/checkin/hooks', () => ({
  useCheckinsByGameDay: vi.fn(),
  useCheckinsByPlayer: vi.fn(),
  useCreateCheckin: vi.fn(),
  useCancelCheckin: vi.fn(),
}))

import {
  useCancelCheckin,
  useCheckinsByGameDay,
  useCheckinsByPlayer,
  useCreateCheckin,
} from '@/features/checkin/hooks'

const createCheckin = vi.fn()
const cancelCheckin = vi.fn()

describe('CheckinsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    useCheckinStore.getState().reset()

    vi.mocked(useCheckinsByGameDay).mockReturnValue({
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
    } as ReturnType<typeof useCheckinsByGameDay>)

    vi.mocked(useCheckinsByPlayer).mockReturnValue({
      data: [],
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

    vi.mocked(useCancelCheckin).mockReturnValue({
      cancelCheckin,
      isPending: false,
      isError: false,
      error: null,
      errorCode: null,
    })
  })

  it('deve renderizar título da página e lista de check-ins', () => {
    render(<CheckinsPage />)

    expect(screen.getByRole('heading', { name: /check-ins/i })).toBeInTheDocument()
    expect(screen.getByText(/player: player-1/i)).toBeInTheDocument()
  })

  it('deve exibir erro de acesso quando query retorna forbidden', () => {
    vi.mocked(useCheckinsByGameDay).mockReturnValue({
      data: [],
      isLoading: false,
      error: {
        response: {
          data: { title: 'FORBIDDEN' },
        },
      },
      isError: true,
    } as ReturnType<typeof useCheckinsByGameDay>)

    render(<CheckinsPage />)

    expect(screen.getByText(/perfil de acesso/i)).toBeInTheDocument()
  })

  it('deve submeter criação de check-in com dados válidos', async () => {
    render(<CheckinsPage />)

    await userEvent.type(
      screen.getByLabelText(/playerid/i),
      '2b6c6402-bb43-4945-bf4f-7df5b91b0a9e',
    )
    await userEvent.type(
      screen.getByLabelText(/gamedayid/i),
      '425cb75f-cf2f-44ec-8682-a94ea8018f5b',
    )
    await userEvent.type(screen.getByLabelText(/latitude/i), '-23.5505')
    await userEvent.type(screen.getByLabelText(/longitude/i), '-46.6333')

    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(createCheckin).toHaveBeenCalledTimes(1)
  })
})
