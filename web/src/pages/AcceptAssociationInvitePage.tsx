import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { invitationService } from '@/features/tenant-invitations/services/invitationService'
import { useAuthStore } from '@/features/auth/store/authStore'
import { authService } from '@/features/auth/services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { setPendingInviteToken } from '@/features/tenant-invitations/utils/pendingInviteStorage'

type Step = 'loading' | 'login' | 'register' | 'success' | 'error'

export function AcceptAssociationInvitePage() {
  const navigate = useNavigate()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const setCurrentTenant = useAuthStore((s) => s.setCurrentTenant)
  const setPlayerOnboardingRequired = useAuthStore((s) => s.setPlayerOnboardingRequired)
  const [step, setStep] = useState<Step>('loading')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [validationError, setValidationError] = useState<string | null>(null)

  const token = useMemo(() => {
    const query = new URLSearchParams(window.location.search)
    return query.get('token')?.trim() ?? ''
  }, [])

  const acceptMutation = useMutation({ mutationFn: invitationService.accept })
  const validateMutation = useMutation({ mutationFn: invitationService.validate })
  const registerAndAcceptMutation = useMutation({
    mutationFn: ({ token, email, password }: { token: string; email: string; password: string }) =>
      invitationService.registerAndAccept(token, email, password),
  })

  useEffect(() => {
    async function run() {
      if (!token) {
        setStep('error')
        setValidationError('ASSOCIATION_INVITE_INVALID_TOKEN')
        return
      }

      if (isAuthenticated) {
        try {
          const accepted = await acceptMutation.mutateAsync(token)
          setCurrentTenant({ slug: accepted.tenantSlug, source: 'profile' })
          setPlayerOnboardingRequired(accepted.requiresPlayerProfile)
          setStep('success')
          void navigate({ to: accepted.requiresPlayerProfile ? '/players/complete-profile' : '/' })
          return
        } catch {
          setStep('error')
          setValidationError(getErrorCode(acceptMutation.error))
          return
        }
      }

      try {
        const validation = await validateMutation.mutateAsync(token)
        if (validation.requiresRegistration) {
          setEmail(validation.email)
          setStep('register')
          return
        }

        setPendingInviteToken(token)
        setStep('login')
        void navigate({ to: '/login' })
      } catch {
        setStep('error')
        setValidationError(getErrorCode(validateMutation.error))
      }
    }

    void run()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, isAuthenticated])

  async function handleRegisterAndAccept(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()

    if (password !== confirmPassword) {
      setValidationError('PASSWORD_CONFIRMATION_MISMATCH')
      return
    }

    try {
      const accepted = await registerAndAcceptMutation.mutateAsync({ token, email, password })
      const auth = await authService.login({ email, password })
      useAuthStore.getState().setTokens(auth)
      setCurrentTenant({ slug: accepted.tenantSlug, source: 'profile' })
      setPlayerOnboardingRequired(accepted.requiresPlayerProfile)
      setStep('success')
      void navigate({ to: accepted.requiresPlayerProfile ? '/players/complete-profile' : '/' })
    } catch {
      setStep('error')
      setValidationError(getErrorCode(registerAndAcceptMutation.error))
    }
  }

  if (step === 'loading') {
    return <main className="min-h-screen grid place-items-center">Validando convite...</main>
  }

  if (step === 'register') {
    return (
      <main className="min-h-screen grid place-items-center p-4">
        <form onSubmit={handleRegisterAndAccept} className="w-full max-w-md space-y-4 bg-white border rounded-xl p-6">
          <h1 className="text-xl font-semibold">Complete seu cadastro para entrar na associação</h1>
          <div>
            <label className="block text-sm mb-1">E-mail</label>
            <input
              className="w-full h-10 px-3 border rounded"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </div>
          <div>
            <label className="block text-sm mb-1">Senha</label>
            <input
              className="w-full h-10 px-3 border rounded"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </div>
          <div>
            <label className="block text-sm mb-1">Confirmar senha</label>
            <input
              className="w-full h-10 px-3 border rounded"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              required
            />
          </div>
          {validationError ? <p className="text-sm text-red-600">{validationError}</p> : null}
          <button type="submit" className="w-full h-10 rounded bg-black text-white" disabled={registerAndAcceptMutation.isPending}>
            {registerAndAcceptMutation.isPending ? 'Processando...' : 'Cadastrar e aceitar convite'}
          </button>
        </form>
      </main>
    )
  }

  if (step === 'error') {
    return (
      <main className="min-h-screen grid place-items-center">
        <div className="text-center space-y-3">
          <h1 className="text-2xl font-semibold">Nao foi possivel aceitar o convite</h1>
          <p className="text-sm text-gray-600">{validationError ?? 'Erro inesperado'}</p>
          <button type="button" className="h-10 px-4 rounded border" onClick={() => navigate({ to: '/login' })}>
            Ir para login
          </button>
        </div>
      </main>
    )
  }

  return <main className="min-h-screen grid place-items-center">Redirecionando...</main>
}
