import { useNavigate, useParams } from '@tanstack/react-router'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { useAssociationStatus } from '@/features/tenant-onboarding/hooks'

const TERMINAL_SUCCESS = 'ready'
const TERMINAL_ERROR = new Set(['failed', 'cancelled'])

interface ProvisioningStep {
  id: string
  title: string
}

const PROVISIONING_STEPS: ProvisioningStep[] = [
  { id: 'requested', title: 'Solicitação recebida' },
  { id: 'queued', title: 'Provisionamento em fila' },
  { id: 'setup', title: 'Configuração do ambiente' },
  { id: 'ready', title: 'Ambiente pronto' },
]

function buildTenantAwareLoginUrl(slug: string): string {
  if (typeof window === 'undefined') {
    return `/login?tenant=${encodeURIComponent(slug)}`
  }

  const { protocol, hostname, port } = window.location
  const normalizedSlug = encodeURIComponent(slug.trim().toLowerCase())
  const isIpHost = /^\d{1,3}(?:\.\d{1,3}){3}$/.test(hostname)
  const isLocalHost = hostname === 'localhost' || hostname.endsWith('.localhost')

  if (isIpHost || isLocalHost) {
    return `/login?tenant=${normalizedSlug}`
  }

  const parts = hostname.split('.').filter(Boolean)
  if (parts.length < 2) {
    return `/login?tenant=${normalizedSlug}`
  }

  const rootDomain = parts.slice(-2).join('.')
  const hostWithTenant = `${decodeURIComponent(normalizedSlug)}.${rootDomain}`
  const portSuffix = port ? `:${port}` : ''
  return `${protocol}//${hostWithTenant}${portSuffix}/login`
}

function getTerminalErrorMessage(status: string): string {
  if (status === 'failed') {
    return 'O provisionamento falhou. Revise os dados informados ou tente criar uma nova associação.'
  }

  if (status === 'cancelled') {
    return 'O provisionamento foi cancelado antes da conclusão. Você pode iniciar um novo cadastro.'
  }

  return 'O provisionamento não foi concluído. Tente iniciar um novo cadastro.'
}

function getCurrentStepIndex(status: string | undefined): number {
  switch (status) {
    case 'ready':
      return 3
    case 'inprogress':
      return 2
    case 'failed':
    case 'cancelled':
      return 2
    case 'pending':
    default:
      return 1
  }
}

function getCurrentStepLabel(status: string | undefined): string {
  switch (status) {
    case 'ready':
      return 'Ambiente pronto para uso'
    case 'inprogress':
      return 'Configuração do ambiente em execução'
    case 'failed':
      return 'Provisionamento falhou durante a configuração'
    case 'cancelled':
      return 'Provisionamento interrompido antes da conclusão'
    case 'pending':
    default:
      return 'Aguardando início da configuração'
  }
}

function ProvisioningTimeline({ status }: { status: string | undefined }) {
  const currentStep = getCurrentStepIndex(status)
  const isTerminalError = Boolean(status && TERMINAL_ERROR.has(status))

  return (
    <section className="mt-5 rounded-xl border border-slate-200 bg-white/70 p-4" aria-label="Etapas do provisionamento">
      <h2 className="text-sm font-semibold text-slate-900">Etapas do provisionamento</h2>
      <p className="mt-1 text-xs text-slate-600">Etapa atual: {getCurrentStepLabel(status)}</p>

      <ol className="mt-4 space-y-2">
        {PROVISIONING_STEPS.map((step, index) => {
          const isCompleted = index < currentStep
          const isCurrent = index === currentStep
          const isErrorStep = isTerminalError && isCurrent

          const indicatorClass = isErrorStep
            ? 'bg-red-500 text-white border-red-500'
            : isCompleted
              ? 'bg-emerald-500 text-white border-emerald-500'
              : isCurrent
                ? 'bg-blue-600 text-white border-blue-600'
                : 'bg-white text-slate-400 border-slate-300'

          const statusText = isErrorStep
            ? 'Falhou'
            : isCompleted
              ? 'Concluida'
              : isCurrent
                ? 'Em andamento'
                : 'Pendente'

          return (
            <li key={step.id} className="flex items-center justify-between gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2">
              <div className="flex items-center gap-3">
                <span
                  className={`inline-flex h-6 w-6 items-center justify-center rounded-full border text-xs font-bold ${indicatorClass}`}
                >
                  {index + 1}
                </span>
                <span className="text-sm font-medium text-slate-800">{step.title}</span>
              </div>
              <span
                className={`text-xs font-semibold ${
                  isErrorStep
                    ? 'text-red-700'
                    : isCompleted
                      ? 'text-emerald-700'
                      : isCurrent
                        ? 'text-blue-700'
                        : 'text-slate-500'
                }`}
              >
                {statusText}
              </span>
            </li>
          )
        })}
      </ol>
    </section>
  )
}

export function AssociationProvisioningStatusPage() {
  const navigate = useNavigate()
  const params = useParams({ strict: false })
  const tenantId = typeof params.tenantId === 'string' ? params.tenantId : ''

  const statusQuery = useAssociationStatus(tenantId)
  const status = statusQuery.data?.provisioningStatus?.toLowerCase()

  if (!tenantId) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <main className="w-full max-w-md rounded-xl border border-amber-300 bg-amber-50 p-6 text-amber-900">
          <h1 className="text-xl font-semibold">Solicitação inválida</h1>
          <p className="mt-2 text-sm">Não foi possível identificar a associação para acompanhamento.</p>
          <button
            type="button"
            className="mt-4 h-10 rounded-lg bg-amber-600 px-4 text-sm font-semibold text-white"
            onClick={() => navigate({ to: '/register-association' })}
          >
            Criar nova associação
          </button>
        </main>
      </div>
    )
  }

  if (statusQuery.isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <main className="w-full max-w-md rounded-xl border border-gray-200 bg-white p-6">
          <h1 className="text-xl font-semibold text-gray-900">Provisionando associação</h1>
          <p className="mt-2 text-sm text-gray-600">Aguardando atualização de status...</p>
          <ProvisioningTimeline status="pending" />
        </main>
      </div>
    )
  }

  if (statusQuery.isError) {
    const errorCode = getErrorCode(statusQuery.error)
    const isNotFound = errorCode === ERROR_CODES.TENANT_NOT_FOUND

    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <main className="w-full max-w-md rounded-xl border border-red-300 bg-red-50 p-6 text-red-900">
          <h1 className="text-xl font-semibold">Falha ao consultar associação</h1>
          <p className="mt-2 text-sm">
            {isNotFound
              ? 'A associação informada não foi encontrada.'
              : 'Não foi possível consultar o status no momento.'}
          </p>
          <button
            type="button"
            className="mt-4 h-10 rounded-lg bg-red-600 px-4 text-sm font-semibold text-white"
            onClick={() => statusQuery.refetch()}
          >
            Tentar novamente
          </button>
        </main>
      </div>
    )
  }

  if (status === TERMINAL_SUCCESS) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <main className="w-full max-w-md rounded-xl border border-emerald-300 bg-emerald-50 p-6 text-emerald-900">
          <h1 className="text-xl font-semibold">Associação pronta</h1>
          <p className="mt-2 text-sm">
            Provisionamento concluído para <strong>{statusQuery.data?.name}</strong>.
          </p>
          <ProvisioningTimeline status={status} />
          <button
            type="button"
            className="mt-4 h-10 rounded-lg bg-emerald-600 px-4 text-sm font-semibold text-white"
            onClick={() => {
              const slug = statusQuery.data?.slug
              if (slug) {
                const targetUrl = buildTenantAwareLoginUrl(slug)
                window.location.assign(targetUrl)
                return
              }

              void navigate({ to: '/login' })
            }}
          >
            Ir para login
          </button>
        </main>
      </div>
    )
  }

  if (status && TERMINAL_ERROR.has(status)) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <main className="w-full max-w-md rounded-xl border border-red-300 bg-red-50 p-6 text-red-900">
          <h1 className="text-xl font-semibold">Provisionamento não concluído</h1>
          <p className="mt-2 text-sm">Status atual: {statusQuery.data?.provisioningStatus}</p>
          <p className="mt-1 text-sm">{getTerminalErrorMessage(status)}</p>
          <ProvisioningTimeline status={status} />
          <button
            type="button"
            className="mt-4 h-10 rounded-lg bg-gray-900 px-4 text-sm font-semibold text-white"
            onClick={() => navigate({ to: '/register-association' })}
          >
            Criar outra associação
          </button>
        </main>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <main className="w-full max-w-md rounded-xl border border-blue-300 bg-blue-50 p-6 text-blue-900">
        <h1 className="text-xl font-semibold">Provisionamento em andamento</h1>
        <p className="mt-2 text-sm">Status atual: {statusQuery.data?.provisioningStatus ?? 'Pending'}</p>
        <p className="mt-1 text-sm">Atualizamos automaticamente esta página a cada poucos segundos.</p>
        <ProvisioningTimeline status={status} />
      </main>
    </div>
  )
}
