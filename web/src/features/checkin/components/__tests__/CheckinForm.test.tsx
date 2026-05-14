import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { FormEvent } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { CheckinForm } from '../CheckinForm'

describe('CheckinForm', () => {
  it('deve chamar submit e localização quando acionado', async () => {
    const onSubmit = vi.fn((event: FormEvent<HTMLFormElement>) => event.preventDefault())
    const onUseCurrentLocation = vi.fn()

    render(
      <CheckinForm
        gameDayId=""
        gameDayOptions={[
          { id: 'gameday-1', scheduledAt: '2026-05-05T10:00:00.000Z', status: 'Confirmed' },
        ]}
        isGameDaysLoading={false}
        latitude=""
        longitude=""
        isSubmitting={false}
        formError={null}
        apiErrorMessage={null}
        onGameDayIdChange={vi.fn()}
        onLatitudeChange={vi.fn()}
        onLongitudeChange={vi.fn()}
        onUseCurrentLocation={onUseCurrentLocation}
        onSubmit={onSubmit}
      />,
    )

    await userEvent.click(screen.getByRole('button', { name: /usar minha localização/i }))
    await userEvent.click(screen.getByRole('button', { name: /registrar check-in/i }))

    expect(onUseCurrentLocation).toHaveBeenCalledTimes(1)
    expect(onSubmit).toHaveBeenCalledTimes(1)
  })

  it('deve renderizar mensagens de erro', () => {
    render(
      <CheckinForm
        gameDayId=""
        gameDayOptions={[]}
        isGameDaysLoading={false}
        latitude=""
        longitude=""
        isSubmitting={false}
        formError="Erro de validação"
        apiErrorMessage="Erro da API"
        onGameDayIdChange={vi.fn()}
        onLatitudeChange={vi.fn()}
        onLongitudeChange={vi.fn()}
        onUseCurrentLocation={vi.fn()}
        onSubmit={vi.fn()}
      />,
    )

    expect(screen.getByText('Erro de validação')).toBeInTheDocument()
    expect(screen.getByText('Erro da API')).toBeInTheDocument()
  })
})
