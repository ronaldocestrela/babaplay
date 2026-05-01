import { useLogin } from '@/features/auth/hooks/useLogin'
import { LoginForm } from '@/features/auth/components/LoginForm'
import type { LoginFormValues } from '@/features/auth/schemas/loginSchema'

export function LoginPage() {
  const { login, isPending, errorCode } = useLogin()

  return (
    <div className="min-h-screen flex items-center justify-center px-4">
      <div className="w-full max-w-sm bg-white rounded-2xl shadow-md p-8">
        <h1 className="text-2xl font-bold text-center text-gray-900 mb-2">BabaPlay</h1>
        <p className="text-center text-sm text-gray-500 mb-6">
          Acesse sua conta para continuar
        </p>
        <LoginForm
          onSubmit={(data: LoginFormValues) => login(data)}
          isLoading={isPending}
          errorCode={errorCode}
        />
      </div>
    </div>
  )
}
