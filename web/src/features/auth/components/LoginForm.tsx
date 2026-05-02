import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { loginSchema, type LoginFormValues } from '../schemas/loginSchema'
import { ERROR_CODES } from '@/core/constants/errorCodes'

interface LoginFormProps {
  onSubmit: (data: LoginFormValues) => void
  isLoading: boolean
  errorCode?: string | null
}

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.INVALID_CREDENTIALS]: 'Email ou senha inválidos.',
  [ERROR_CODES.USER_INACTIVE]: 'Usuário inativo. Entre em contato com o administrador.',
}

export function LoginForm({ onSubmit, isLoading, errorCode }: LoginFormProps) {
  const [showPassword, setShowPassword] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
      {/* Email */}
      <div className="space-y-2">
        <label
          htmlFor="email"
          className="text-sm font-medium text-on-surface flex items-center gap-2"
        >
          <span className="material-symbols-outlined text-[18px]">mail</span>
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          placeholder="admin@babaplay.com"
          {...register('email')}
          aria-invalid={!!errors.email}
          className="w-full h-12 px-4 rounded-lg border border-outline-variant bg-surface-container-lowest text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary-container focus:border-primary-container transition-all placeholder:text-outline-variant aria-[invalid=true]:border-error"
        />
        {errors.email && (
          <p role="alert" className="text-xs text-error mt-1">
            {errors.email.message}
          </p>
        )}
      </div>

      {/* Password */}
      <div className="space-y-2">
        <div className="flex justify-between items-center">
          <label
            htmlFor="password"
            className="text-sm font-medium text-on-surface flex items-center gap-2"
          >
            <span className="material-symbols-outlined text-[18px]">lock</span>
            Senha
          </label>
          <a
            href="#"
            className="text-xs font-semibold text-primary hover:text-on-primary-container transition-colors"
          >
            Esqueceu a senha?
          </a>
        </div>
        <div className="relative">
          <input
            id="password"
            type={showPassword ? 'text' : 'password'}
            autoComplete="current-password"
            placeholder="••••••••"
            {...register('password')}
            aria-invalid={!!errors.password}
            className="w-full h-12 px-4 pr-11 rounded-lg border border-outline-variant bg-surface-container-lowest text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary-container focus:border-primary-container transition-all placeholder:text-outline-variant aria-[invalid=true]:border-error"
          />
          <button
            type="button"
            aria-label={showPassword ? 'Ocultar' : 'Mostrar'}
            onClick={() => setShowPassword((v) => !v)}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface-variant transition-colors"
          >
            <span className="material-symbols-outlined text-[20px]">
              {showPassword ? 'visibility_off' : 'visibility'}
            </span>
          </button>
        </div>
        {errors.password && (
          <p role="alert" className="text-xs text-error mt-1">
            {errors.password.message}
          </p>
        )}
      </div>

      {/* Remember me */}
      <div className="flex items-center gap-3">
        <input
          id="remember"
          type="checkbox"
          className="w-5 h-5 rounded border-outline-variant text-primary focus:ring-primary-container cursor-pointer"
        />
        <label
          htmlFor="remember"
          className="text-sm text-on-surface-variant cursor-pointer select-none"
        >
          Lembrar de mim
        </label>
      </div>

      {errorCode && (
        <p role="alert" aria-live="polite" className="text-sm text-error text-center">
          {ERROR_MESSAGES[errorCode] ?? 'Ocorreu um erro. Tente novamente.'}
        </p>
      )}

      <button
        type="submit"
        disabled={isLoading}
        className="w-full h-12 bg-primary text-on-primary font-semibold text-base rounded-lg shadow-sm hover:bg-on-primary-container active:scale-[0.98] transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {isLoading ? 'Entrando...' : (
          <>
            Entrar
            <span className="material-symbols-outlined text-[20px]">arrow_forward</span>
          </>
        )}
      </button>
    </form>
  )
}

