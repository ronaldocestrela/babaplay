import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from '@tanstack/react-router'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import {
  associationFormSchema,
  type AssociationFormValues,
} from '@/features/tenant-onboarding/schemas/associationFormSchema'
import { useCreateAssociation } from '@/features/tenant-onboarding/hooks'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.TENANT_NAME_REQUIRED]: 'Nome da associação é obrigatório.',
  [ERROR_CODES.TENANT_SLUG_REQUIRED]: 'Slug é obrigatório.',
  [ERROR_CODES.TENANT_SLUG_TAKEN]: 'Este slug já está em uso.',
}

export function RegisterAssociationPage() {
  const navigate = useNavigate()
  const [apiError, setApiError] = useState<string | null>(null)
  const { createAssociation, isPending, errorCode } = useCreateAssociation()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AssociationFormValues>({
    resolver: zodResolver(associationFormSchema),
    defaultValues: {
      name: '',
      slug: '',
    },
  })

  const onSubmit = (data: AssociationFormValues) => {
    setApiError(null)

    createAssociation(data, {
      onSuccess: (response) => {
        void navigate({
          to: '/register-association/status/$tenantId',
          params: { tenantId: response.id },
        })
      },
      onError: () => {
        setApiError(ERROR_MESSAGES[errorCode ?? ''] ?? 'Falha ao criar associação.')
      },
    })
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-4 md:p-8">
      <main className="w-full max-w-md rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-semibold text-gray-900">Registrar Nova Associação</h1>
        <p className="mt-1 text-sm text-gray-600">
          Crie sua associação para iniciar o provisionamento do ambiente.
        </p>

        <form className="mt-6 space-y-4" onSubmit={handleSubmit(onSubmit)}>
          {apiError && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
              {apiError}
            </div>
          )}

          <div>
            <label htmlFor="association-name" className="mb-1 block text-sm font-medium text-gray-700">
              Nome da associação
            </label>
            <input
              id="association-name"
              type="text"
              {...register('name')}
              placeholder="Ex.: Associação Esportiva Central"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.name && <p className="mt-1 text-xs text-red-600">{errors.name.message}</p>}
          </div>

          <div>
            <label htmlFor="association-slug" className="mb-1 block text-sm font-medium text-gray-700">
              Slug
            </label>
            <input
              id="association-slug"
              type="text"
              {...register('slug')}
              placeholder="Ex.: associacao-central"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.slug && <p className="mt-1 text-xs text-red-600">{errors.slug.message}</p>}
          </div>

          <button
            type="submit"
            className="h-11 w-full rounded-lg bg-indigo-600 text-sm font-semibold text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={isPending}
          >
            {isPending ? 'Criando associação...' : 'Criar associação'}
          </button>

          <button
            type="button"
            className="h-11 w-full rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50"
            onClick={() => navigate({ to: '/login' })}
            disabled={isPending}
          >
            Voltar para login
          </button>
        </form>
      </main>
    </div>
  )
}
