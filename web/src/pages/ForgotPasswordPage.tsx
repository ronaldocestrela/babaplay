import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Link } from '@tanstack/react-router'
import { authService } from '@/features/auth/services/authService'
import { getErrorCode } from '@/core/utils/getErrorCode'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [submitted, setSubmitted] = useState(false)

  const { mutate: sendResetEmail, isPending, error, isError } = useMutation({
    mutationFn: () => {
      // Send the request. We construct the resetLinkBaseUrl as current host + "/reset-password"
      const resetLinkBaseUrl = `${window.location.origin}/reset-password`
      return authService.forgotPassword({ email, resetLinkBaseUrl })
    },
    onSuccess: () => {
      setSubmitted(true)
    },
  })

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!email) return
    sendResetEmail()
  }

  const errorCode = getErrorCode(error)

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

        {/* Forgot Password card */}
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] p-6 md:p-12">
          {!submitted ? (
            <>
              <div className="mb-6">
                <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">Recuperar Senha</h2>
                <p className="text-sm text-on-surface-variant mt-0.5">
                  Informe o seu e-mail para receber um link de redefinição de senha.
                </p>
              </div>

              <form onSubmit={handleSubmit} noValidate className="space-y-6">
                {/* Email Field */}
                <div className="space-y-2">
                  <label htmlFor="email" className="text-sm font-medium text-on-surface flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">mail</span>
                    E-mail
                  </label>
                  <input
                    id="email"
                    type="email"
                    placeholder="admin@babaplay.com"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                    className="w-full h-12 px-4 rounded-lg border border-outline-variant bg-surface-container-lowest text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary-container focus:border-primary-container transition-all placeholder:text-outline-variant"
                  />
                </div>

                {isError && (
                  <p role="alert" className="text-sm text-error text-center">
                    {errorCode === 'EMAIL_REQUIRED' ? 'O e-mail é obrigatório.' : 'Erro ao solicitar redefinição. Tente novamente.'}
                  </p>
                )}

                <button
                  type="submit"
                  disabled={isPending}
                  className="w-full h-12 bg-primary text-on-primary font-semibold text-base rounded-lg shadow-sm hover:bg-on-primary-container active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isPending ? 'Enviando...' : (
                    <>
                      Enviar Link de Recuperação
                      <span className="material-symbols-outlined text-[20px]">send</span>
                    </>
                  )}
                </button>
              </form>
            </>
          ) : (
            <div className="text-center space-y-6">
              <div className="w-16 h-16 bg-success/10 text-success rounded-full flex items-center justify-center mx-auto">
                <span className="material-symbols-outlined text-[36px]">mail_outline</span>
              </div>
              <div className="space-y-2">
                <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">E-mail Enviado!</h2>
                <p className="text-sm text-on-surface-variant">
                  Se o e-mail <strong>{email}</strong> estiver registrado em nossa plataforma, você receberá em instantes um link para redefinir sua senha.
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
