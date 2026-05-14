import { useState } from 'react'
import { useDashboardData } from '@/features/dashboard/hooks/useDashboardData'

type PeriodMode = 'current' | 'custom'

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(value)
}

export function DashboardPage() {
  const [periodMode, setPeriodMode] = useState<PeriodMode>('current')
  const [customFromDate, setCustomFromDate] = useState('')
  const [customToDate, setCustomToDate] = useState('')
  const [appliedPeriod, setAppliedPeriod] = useState<{ fromUtc: string; toUtc: string } | undefined>(
    undefined,
  )

  const selectedPeriod = periodMode === 'custom' ? appliedPeriod : undefined
  const { data, isLoading, isError } = useDashboardData(selectedPeriod)

  const applyCustomPeriod = () => {
    if (!customFromDate || !customToDate) {
      return
    }

    setAppliedPeriod({
      fromUtc: new Date(`${customFromDate}T00:00:00.000Z`).toISOString(),
      toUtc: new Date(`${customToDate}T23:59:59.999Z`).toISOString(),
    })
  }

  const useCurrentMonth = () => {
    setPeriodMode('current')
    setAppliedPeriod(undefined)
  }

  if (isLoading) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-on-surface-variant">Carregando dashboard...</p>
      </div>
    )
  }

  if (isError || !data) {
    return (
      <div className="p-8 max-w-6xl mx-auto">
        <p className="text-error">Não foi possível carregar o dashboard.</p>
      </div>
    )
  }

  return (
    <div className="p-6 md:p-8 max-w-6xl mx-auto space-y-6">
      <header>
        <h1 className="text-2xl font-semibold text-on-surface">Dashboard</h1>
        <p className="text-sm text-on-surface-variant">Visão rápida operacional, ranking e financeiro</p>
      </header>

      <section className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
        <div className="flex flex-wrap items-end gap-3">
          <div className="flex gap-2">
            <button
              type="button"
              onClick={useCurrentMonth}
              className={`px-3 py-2 rounded-lg border text-sm transition ${
                periodMode === 'current'
                  ? 'bg-primary text-white border-primary'
                  : 'bg-surface border-outline-variant text-on-surface'
              }`}
            >
              Mês atual
            </button>
            <button
              type="button"
              onClick={() => setPeriodMode('custom')}
              className={`px-3 py-2 rounded-lg border text-sm transition ${
                periodMode === 'custom'
                  ? 'bg-primary text-white border-primary'
                  : 'bg-surface border-outline-variant text-on-surface'
              }`}
            >
              Personalizado
            </button>
          </div>

          {periodMode === 'custom' ? (
            <>
              <div className="flex flex-col gap-1">
                <label htmlFor="period-from" className="text-xs text-on-surface-variant">
                  De
                </label>
                <input
                  id="period-from"
                  type="date"
                  className="h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
                  value={customFromDate}
                  onChange={(event) => setCustomFromDate(event.target.value)}
                />
              </div>

              <div className="flex flex-col gap-1">
                <label htmlFor="period-to" className="text-xs text-on-surface-variant">
                  Até
                </label>
                <input
                  id="period-to"
                  type="date"
                  className="h-10 px-3 rounded-lg border border-outline-variant bg-surface text-on-surface"
                  value={customToDate}
                  onChange={(event) => setCustomToDate(event.target.value)}
                />
              </div>

              <button
                type="button"
                className="h-10 px-4 rounded-lg border border-primary bg-primary text-white text-sm"
                onClick={applyCustomPeriod}
              >
                Aplicar período
              </button>
            </>
          ) : null}
        </div>
      </section>

      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
          <p className="text-xs text-on-surface-variant">Jogadores ativos</p>
          <p className="text-2xl font-semibold text-on-surface">{data.operational.activePlayers}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
          <p className="text-xs text-on-surface-variant">Times ativos</p>
          <p className="text-2xl font-semibold text-on-surface">{data.operational.activeTeams}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
          <p className="text-xs text-on-surface-variant">Próximos game days</p>
          <p className="text-2xl font-semibold text-on-surface">{data.operational.upcomingGameDays}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
          <p className="text-xs text-on-surface-variant">Partidas ao vivo</p>
          <p className="text-2xl font-semibold text-on-surface">{data.operational.liveMatches}</p>
        </div>
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl p-4">
          <p className="text-xs text-on-surface-variant">Check-ins hoje</p>
          <p className="text-2xl font-semibold text-on-surface">{data.operational.todayCheckins}</p>
        </div>
      </section>

      <section className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <article className="bg-surface-container-lowest border border-outline-variant rounded-xl p-5">
          <h2 className="text-lg font-semibold text-on-surface mb-3">Ranking</h2>

          {data.ranking.available ? (
            <div className="space-y-3">
              <div>
                <p className="text-xs text-on-surface-variant">Melhor score</p>
                <p className="text-2xl font-semibold text-on-surface">{data.ranking.bestScore}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-on-surface">Top artilharia</p>
                <ul className="text-sm text-on-surface-variant mt-1">
                  {data.ranking.topScorers.map((entry) => (
                    <li key={`${entry.playerId}-${entry.rank}`}>#{entry.rank} • {entry.playerId} • {entry.goals} gols</li>
                  ))}
                </ul>
              </div>
              <div>
                <p className="text-sm font-medium text-on-surface">Top presença</p>
                <ul className="text-sm text-on-surface-variant mt-1">
                  {data.ranking.attendanceLeaders.map((entry) => (
                    <li key={`${entry.playerId}-${entry.rank}`}>#{entry.rank} • {entry.playerId} • {entry.attendanceCount} presenças</li>
                  ))}
                </ul>
              </div>
            </div>
          ) : (
            <p className="text-sm text-on-surface-variant">Ranking indisponível no seu perfil de acesso.</p>
          )}
        </article>

        <article className="bg-surface-container-lowest border border-outline-variant rounded-xl p-5">
          <h2 className="text-lg font-semibold text-on-surface mb-3">Financeiro</h2>

          {data.financial.available ? (
            <div className="space-y-3">
              <div>
                <p className="text-xs text-on-surface-variant">Saldo em caixa</p>
                <p className="text-2xl font-semibold text-on-surface">
                  {formatCurrency(data.financial.cashBalance)}
                </p>
              </div>
              <div className="text-sm text-on-surface-variant">
                <p>Em aberto: {formatCurrency(data.financial.openAmount)}</p>
                <p>Mensalidades pagas: {formatCurrency(data.financial.monthlyFeesPaidAmount)}</p>
              </div>
            </div>
          ) : (
            <p className="text-sm text-on-surface-variant">Financeiro indisponível no seu perfil de acesso.</p>
          )}
        </article>
      </section>
    </div>
  )
}
