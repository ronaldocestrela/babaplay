import { useLogin } from '@/features/auth/hooks/useLogin'
import { LoginForm } from '@/features/auth/components/LoginForm'
import type { LoginFormValues } from '@/features/auth/schemas/loginSchema'

export function LoginPage() {
  const { login, isPending, errorCode } = useLogin()

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

      {/* Side decoration – left */}
      <div className="hidden xl:block absolute left-6 top-1/2 -translate-y-1/2 w-64 h-96 overflow-hidden rounded-xl border border-outline-variant shadow-lg -rotate-2">
        <img
          alt="Sports Management"
          className="w-full h-full object-cover grayscale opacity-40"
          src="https://lh3.googleusercontent.com/aida-public/AB6AXuBcJj3NTYx-kqIF69BZSltDho_LjFw1s1rs2z1L1m8qNYl9zZlSky4VQsU4XnCZGmGya3l7NkrebUHbKhAKeEo3uwzx6QCoz7tIz6XWgsYy4bG027R3REd_gSXPAu0yqEVR1_2H3ksdgjC4f1mEyAM73upK55NWSZy9jq142DZbVadLbpJMY3ijxdNETxpUlk-zPlklEarxApzXT06sIkD8qKXPsYeHKO9a3V4B5qrmvRFGrMd3ehW_EuXN2t2DApI6uuoPzdzNuJU"
        />
      </div>

      {/* Side decoration – right */}
      <div className="hidden xl:block absolute right-6 top-1/2 -translate-y-1/2 w-64 h-96 overflow-hidden rounded-xl border border-outline-variant shadow-lg rotate-2">
        <img
          alt="Athlete Statistics"
          className="w-full h-full object-cover grayscale opacity-40"
          src="https://lh3.googleusercontent.com/aida-public/AB6AXuCL0V6s6y4dII5YAe3R38Lx9y2WoHse9OOL3wBSoHvuOaXJXdRqy2Nd1bLONzibzdCqZ6Ic1zv6VSqTNQEegEKVSqACiou--_OZ5U3uCdjuBHhUzgfbHdmU-7GA6teIctySymg4YCqZ0neC7qc8N6zvY62qb30NxjgzVI2Teyex7h38375hSYq1QPn5Y1H6NTuURixd92Nmg0zTqK-X8_NKl1rqsHbHPbfzRWe7qdOwxJOhjBxG8VdgVDH1ERBZQomblQYPgRQH0UA"
        />
      </div>

      {/* Main container */}
      <main className="w-full max-w-[440px] relative z-10">
        {/* Brand identity */}
        <div className="flex flex-col items-center mb-12">
          <div className="w-16 h-16 bg-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary-container/20 mb-4">
            <span className="material-symbols-outlined text-white text-[32px]">sports_soccer</span>
          </div>
          <h1 className="font-[Lexend] text-3xl font-semibold text-on-surface tracking-tight">BabaPlay</h1>
          <p className="text-sm text-on-surface-variant mt-1">Administrative Excellence in Sports</p>
        </div>

        {/* Login card */}
        <div className="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] p-6 md:p-12">
          <div className="mb-6">
            <h2 className="font-[Lexend] text-2xl font-semibold text-on-surface">Sign In</h2>
            <p className="text-sm text-on-surface-variant mt-0.5">Access your administrative portal</p>
          </div>

          <LoginForm
            onSubmit={(data: LoginFormValues) => login(data)}
            isLoading={isPending}
            errorCode={errorCode}
          />

          <div className="mt-8 pt-6 border-t border-outline-variant flex flex-col items-center gap-4">
            <p className="text-sm text-on-surface-variant">Não tem uma conta de associação?</p>
            <button
              type="button"
              className="w-full h-11 border border-outline-variant text-on-surface text-sm font-medium rounded-lg hover:bg-surface-container-high active:bg-surface-container-highest transition-all"
            >
              Registrar Nova Associação
            </button>
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

