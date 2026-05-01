import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ErrorBoundary } from '../ErrorBoundary'

// Componente que lança erro deliberadamente
function ThrowingComponent({ shouldThrow }: { shouldThrow: boolean }) {
  if (shouldThrow) throw new Error('Erro de renderização deliberado')
  return <div>Conteúdo normal</div>
}

describe('ErrorBoundary', () => {
  beforeEach(() => {
    // Suprimir console.error de erros React esperados nos testes
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  it('deve renderizar children quando não há erro', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent shouldThrow={false} />
      </ErrorBoundary>,
    )
    expect(screen.getByText('Conteúdo normal')).toBeInTheDocument()
  })

  it('deve exibir fallback quando filho lança erro', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent shouldThrow={true} />
      </ErrorBoundary>,
    )
    expect(screen.getByRole('heading', { name: /algo deu errado/i })).toBeInTheDocument()
  })

  it('deve exibir botão para recarregar quando há erro', () => {
    render(
      <ErrorBoundary>
        <ThrowingComponent shouldThrow={true} />
      </ErrorBoundary>,
    )
    expect(screen.getByRole('button', { name: /recarregar/i })).toBeInTheDocument()
  })

  it('deve chamar window.location.reload ao clicar em recarregar', async () => {
    const reloadMock = vi.fn()
    Object.defineProperty(window, 'location', {
      value: { ...window.location, reload: reloadMock },
      writable: true,
    })

    render(
      <ErrorBoundary>
        <ThrowingComponent shouldThrow={true} />
      </ErrorBoundary>,
    )

    await userEvent.click(screen.getByRole('button', { name: /recarregar/i }))
    expect(reloadMock).toHaveBeenCalledOnce()
  })
})
