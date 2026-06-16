import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from '@tanstack/react-router'
import { invitationService } from '@/features/tenant-invitations/services/invitationService'
import { useAuthStore } from '@/features/auth/store/authStore'
import { authService } from '@/features/auth/services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'
import { setPendingInviteToken } from '@/features/tenant-invitations/utils/pendingInviteStorage'

const ERROR_MESSAGES: Record<string, string> = {
  PASSWORD_CONFIRMATION_MISMATCH: 'As senhas informadas não coincidem.',
  PASSWORD_TOO_SHORT: 'A senha deve conter no mínimo 8 caracteres.',
  PASSWORD_REQUIRES_UPPERCASE: 'A senha deve conter ao menos uma letra maiúscula.',
  PASSWORD_REQUIRES_DIGIT: 'A senha deve conter ao menos um número.',
  ASSOCIATION_INVITE_INVALID_TOKEN: 'Token de convite inválido ou expirado.',
  ASSOCIATION_INVITE_TOKEN_EXPIRED: 'Token de convite expirado.',
  ASSOCIATION_INVITE_ALREADY_USED: 'Este convite já foi utilizado.',
  ASSOCIATION_INVITE_ALREADY_REVOKED: 'Este convite foi revogado.',
  ASSOCIATION_INVITE_EMAIL_MISMATCH: 'O e-mail informado não corresponde ao e-mail do convite.',
  ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED: 'Este e-mail já está cadastrado.',
  ASSOCIATION_INVITE_PASSWORD_REQUIRED: 'A senha é obrigatória.',
}

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

  const hasMinLength = password.length >= 8
  const hasUppercase = /[A-Z]/.test(password)
  const hasNumber = /[0-9]/.test(password)

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

    if (password.length < 8) {
      setValidationError('PASSWORD_TOO_SHORT')
      return
    }

    if (!/[A-Z]/.test(password)) {
      setValidationError('PASSWORD_REQUIRES_UPPERCASE')
      return
    }

    if (!/[0-9]/.test(password)) {
      setValidationError('PASSWORD_REQUIRES_DIGIT')
      return
    }

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
        <form onSubmit={handleRegisterAndAccept} className="w-full max-w-md space-y-4 bg-white border border-outline-variant rounded-xl p-6 shadow-sm">
          <h1 className="text-xl font-semibold text-on-surface">Complete seu cadastro para entrar na associação</h1>
          <div>
            <label className="block text-sm mb-1 text-on-surface">E-mail</label>
            <input
              className="w-full h-10 px-3 border border-outline-variant bg-surface rounded text-on-surface focus:outline-none"
              type="email"
              value={email}
              onChange={(event) => {
                setValidationError(null)
                setEmail(event.target.value)
              }}
              required
            />
          </div>
          <div>
            <label className="block text-sm mb-1 text-on-surface">Senha</label>
            <input
              className="w-full h-10 px-3 border border-outline-variant bg-surface rounded text-on-surface focus:outline-none"
              type="password"
              value={password}
              onChange={(event) => {
                setValidationError(null)
                setPassword(event.target.value)
              }}
              required
            />
            
            {/* Password rules box */}
            <div className="mt-2 space-y-1.5 p-3 bg-surface-container-low rounded-lg border border-outline-variant/30">
              <p className="text-xs font-semibold text-on-surface">A senha deve conter:</p>
              <ul className="text-xs space-y-1">
                <li className={`flex items-center gap-1.5 ${hasMinLength ? 'text-primary font-medium' : 'text-on-surface-variant'}`}>
                  <span className="material-symbols-outlined text-[16px] select-none">
                    {hasMinLength ? 'check_circle' : 'radio_button_unchecked'}
                  </span>
                  <span>Mínimo de 8 caracteres</span>
                </li>
                <li className={`flex items-center gap-1.5 ${hasUppercase ? 'text-primary font-medium' : 'text-on-surface-variant'}`}>
                  <span className="material-symbols-outlined text-[16px] select-none">
                    {hasUppercase ? 'check_circle' : 'radio_button_unchecked'}
                  </span>
                  <span>Pelo menos uma letra maiúscula</span>
                </li>
                <li className={`flex items-center gap-1.5 ${hasNumber ? 'text-primary font-medium' : 'text-on-surface-variant'}`}>
                  <span className="material-symbols-outlined text-[16px] select-none">
                    {hasNumber ? 'check_circle' : 'radio_button_unchecked'}
                  </span>
                  <span>Pelo menos um número</span>
                </li>
              </ul>
            </div>
          </div>
          <div>
            <label className="block text-sm mb-1 text-on-surface">Confirmar senha</label>
            <input
              className="w-full h-10 px-3 border border-outline-variant bg-surface rounded text-on-surface focus:outline-none"
              type="password"
              value={confirmPassword}
              onChange={(event) => {
                setValidationError(null)
                setConfirmPassword(event.target.value)
              }}
              required
            />
          </div>
          {validationError ? (
            <p className="text-sm text-error font-medium">
              {ERROR_MESSAGES[validationError] ?? validationError}
            </p>
          ) : null}
          <button type="submit" className="w-full h-10 rounded bg-primary text-white font-semibold hover:opacity-90 active:scale-[0.99] transition-all" disabled={registerAndAcceptMutation.isPending}>
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
          <h1 className="text-2xl font-semibold text-on-surface">Não foi possível aceitar o convite</h1>
          <p className="text-sm text-error font-medium">
            {ERROR_MESSAGES[validationError ?? ''] ?? (validationError ?? 'Erro inesperado')}
          </p>
          <button type="button" className="h-10 px-4 rounded border border-outline-variant text-on-surface hover:bg-surface-container" onClick={() => navigate({ to: '/login' })}>
            Ir para login
          </button>
        </div>
      </main>
    )
  }

  return <main className="min-h-screen grid place-items-center">Redirecionando...</main>
}
