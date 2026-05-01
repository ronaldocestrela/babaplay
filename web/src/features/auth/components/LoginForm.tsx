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
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-4">
      <div>
        <label
          htmlFor="email"
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          {...register('email')}
          aria-invalid={!!errors.email}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 aria-[invalid=true]:border-red-500"
        />
        {errors.email && (
          <p role="alert" className="mt-1 text-xs text-red-600">
            {errors.email.message}
          </p>
        )}
      </div>

      <div>
        <label
          htmlFor="password"
          className="block text-sm font-medium text-gray-700 mb-1"
        >
          Senha
        </label>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          {...register('password')}
          aria-invalid={!!errors.password}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 aria-[invalid=true]:border-red-500"
        />
        {errors.password && (
          <p role="alert" className="mt-1 text-xs text-red-600">
            {errors.password.message}
          </p>
        )}
      </div>

      {errorCode && (
        <p role="alert" aria-live="polite" className="text-sm text-red-600 text-center">
          {ERROR_MESSAGES[errorCode] ?? 'Ocorreu um erro. Tente novamente.'}
        </p>
      )}

      <button
        type="submit"
        disabled={isLoading}
        className="w-full rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        {isLoading ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  )
}
