import { useState, useMemo } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Link } from '@tanstack/react-router'
import { authService } from '@/features/auth/services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'

export function ResetPasswordPage() {
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const [submitted, setSubmitted] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)

  const params = useMemo(() => {
    const query = new URLSearchParams(window.location.search)
    return {
      token: query.get('token')?.trim() ?? '',
      email: query.get('email')?.trim() ?? '',
    }
  }, [])

  const { mutate: resetPassword, isPending, error, isError } = useMutation({
    mutationFn: () => {
      return authService.resetPassword({
        email: params.email,
        token: params.token,
        password,
      })
    },
    onSuccess: () => {
      setSubmitted(true)
    },
  })

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setValidationError(null)

    if (!params.token || !params.email) {
      setValidationError('TOKEN_OR_EMAIL_MISSING')
      return
    }

    if (password.length < 8) {
      setValidationError('PASSWORD_TOO_SHORT')
      return
    }

    if (password !== confirmPassword) {
      setValidationError('PASSWORDS_DONT_MATCH')
      return
    }

    resetPassword()
  }

  const errorCode = getErrorCode(error)

  const hasParamsError = !params.token || !params.email

  return (
    <div className="bg-surface min-h-screen flex items-center justify-center p-4 md:p-10 relative overflow-hidden">
      {/* Background dot texture */}
      <div
        className="absolute inset-0 pointer-events-none"
        style={{
          backgroundImage: 'radial-gradient(#6c7a71 0.5px, transparent 0.5px)',
          backgroundSize: '24px 24px',
          opacity: 0.08,
        }}
      />

      {/* Gradient blobs */}
      <div className="absolute top-[-10%] right-[-10%] w-[500px] h-[500px] rounded-full bg-primary-container/10 blur-[120px] pointer-events-none" />
      <div className="absolute bottom-[-10%] left-[-10%] w-[500px] h-[500px] rounded-full bg-secondary-container/10 blur-[120px] pointer-events-none" />

      {/* Main container */}
      <main className="w-full max-w-[440px] relative z-10">
        {/* Brand identity */}
        <div className="flex flex-col items-center mb-12">
          <div className="w-16 h-16 bg-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary-container/20 mb-4">
            <img src="/babaplayicon.png" alt="BabaPlay icon" className="w-10 h-10 object-contain" />
          </div>
          <h1 className="font-[Lexend] text-3xl font-semibold text-on-surface tracking-tight">BabaPlay</h1>
          <p className="text-sm text-on-surface-variant mt-1">Administrative Excellence in Sports</p>
        </div>

        {/* Reset Password card */}
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] p-6 md:p-12">
          {hasParamsError ? (
            <div className="text-center space-y-6">
              <div className="w-16 h-16 bg-error/10 text-error rounded-full flex items-center justify-center mx-auto">
                <span className="material-symbols-outlined text-[36px]">warning</span>
              </div>
              <div className="space-y-2">
                <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">Link Inválido</h2>
                <p className="text-sm text-on-surface-variant">
                  Este link de redefinição de senha está incompleto ou expirado. Por favor, solicite um novo link.
                </p>
              </div>
            </div>
          ) : !submitted ? (
            <>
              <div className="mb-6">
                <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">Nova Senha</h2>
                <p className="text-sm text-on-surface-variant mt-0.5">
                  Configure uma nova senha para a conta <strong>{params.email}</strong>.
                </p>
              </div>

              <form onSubmit={handleSubmit} noValidate className="space-y-6">
                {/* Password Input */}
                <div className="space-y-2">
                  <label htmlFor="password" className="text-sm font-medium text-on-surface flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">lock</span>
                    Senha
                  </label>
                  <div className="relative">
                    <input
                      id="password"
                      type={showPassword ? 'text' : 'password'}
                      placeholder="••••••••"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      required
                      className="w-full h-12 px-4 pr-11 rounded-lg border border-outline-variant bg-surface-container-lowest text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary-container focus:border-primary-container transition-all placeholder:text-outline-variant"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword((v) => !v)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface-variant transition-colors"
                    >
                      <span className="material-symbols-outlined text-[20px]">
                        {showPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                </div>

                {/* Confirm Password Input */}
                <div className="space-y-2">
                  <label htmlFor="confirmPassword" className="text-sm font-medium text-on-surface flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">lock</span>
                    Confirmar Senha
                  </label>
                  <div className="relative">
                    <input
                      id="confirmPassword"
                      type={showConfirmPassword ? 'text' : 'password'}
                      placeholder="••••••••"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      required
                      className="w-full h-12 px-4 pr-11 rounded-lg border border-outline-variant bg-surface-container-lowest text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary-container focus:border-primary-container transition-all placeholder:text-outline-variant"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword((v) => !v)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface-variant transition-colors"
                    >
                      <span className="material-symbols-outlined text-[20px]">
                        {showConfirmPassword ? 'visibility_off' : 'visibility'}
                      </span>
                    </button>
                  </div>
                </div>

                {/* Validation Errors */}
                {validationError && (
                  <p role="alert" className="text-sm text-error text-center">
                    {validationError === 'PASSWORD_TOO_SHORT' && 'A senha deve conter no mínimo 8 caracteres.'}
                    {validationError === 'PASSWORDS_DONT_MATCH' && 'As senhas informadas não coincidem.'}
                    {validationError === 'TOKEN_OR_EMAIL_MISSING' && 'Dados de redefinição ausentes no link.'}
                  </p>
                )}

                {isError && (
                  <p role="alert" className="text-sm text-error text-center">
                    {errorCode === 'RESET_PASSWORD_FAILED'
                      ? 'Não foi possível redefinir. O link pode ter expirado ou a senha não cumpre as regras.'
                      : 'Ocorreu um erro na redefinição. Tente novamente.'}
                  </p>
                )}

                <button
                  type="submit"
                  disabled={isPending}
                  className="w-full h-12 bg-primary text-on-primary font-semibold text-base rounded-lg shadow-sm hover:bg-on-primary-container active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isPending ? 'Redefinindo...' : (
                    <>
                      Redefinir Senha
                      <span className="material-symbols-outlined text-[20px]">check</span>
                    </>
                  )}
                </button>
              </form>
            </>
          ) : (
            <div className="text-center space-y-6">
              <div className="w-16 h-16 bg-success/10 text-success rounded-full flex items-center justify-center mx-auto">
                <span className="material-symbols-outlined text-[36px]">check_circle_outline</span>
              </div>
              <div className="space-y-2">
                <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">Senha Alterada!</h2>
                <p className="text-sm text-on-surface-variant">
                  Sua senha foi redefinida com sucesso. Agora você pode entrar com suas novas credenciais.
                </p>
              </div>
            </div>
          )}

          <div className="mt-8 pt-6 border-t border-outline-variant flex flex-col items-center">
            <Link
              to="/login"
              className="text-sm font-semibold text-primary hover:text-on-primary-container transition-colors flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[18px]">arrow_back</span>
              Voltar para o Login
            </Link>
          </div>
        </div>

        {/* Footer */}
        <footer className="mt-6 flex flex-col items-center gap-1">
          <p className="text-xs text-outline uppercase tracking-widest font-semibold">
            Secure Cloud Infrastructure
          </p>
          <div className="flex items-center gap-2">
            <span className="material-symbols-outlined text-[14px] text-primary">verified_user</span>
            <span className="text-xs text-on-surface-variant">256-bit AES Encryption</span>
          </div>
        </footer>
      </main>
    </div>
  )
}
