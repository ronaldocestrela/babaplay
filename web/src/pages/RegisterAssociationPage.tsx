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
  [ERROR_CODES.TENANT_LOGO_REQUIRED]: 'Logo da associação é obrigatório.',
  [ERROR_CODES.TENANT_LOGO_INVALID_TYPE]: 'Logo deve ser PNG, JPG ou WEBP.',
  [ERROR_CODES.TENANT_LOGO_INVALID_SIZE]: 'Logo deve ter até 2MB.',
  [ERROR_CODES.TENANT_STREET_REQUIRED]: 'Rua é obrigatória.',
  [ERROR_CODES.TENANT_NUMBER_REQUIRED]: 'Número é obrigatório.',
  [ERROR_CODES.TENANT_CITY_REQUIRED]: 'Cidade é obrigatória.',
  [ERROR_CODES.TENANT_STATE_REQUIRED]: 'Estado é obrigatório.',
  [ERROR_CODES.TENANT_ZIPCODE_REQUIRED]: 'CEP é obrigatório.',
  TENANT_ADMIN_CREDENTIALS_REQUIRED: 'Informe as credenciais iniciais do admin da associação.',
  TENANT_ADMIN_USER_CREATE_FAILED: 'Não foi possível criar o usuário admin inicial.',
  TENANT_ADMIN_INVALID_PASSWORD: 'Credenciais do admin inválidas para o email informado.',
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
      street: '',
      number: '',
      neighborhood: '',
      city: '',
      state: '',
      zipCode: '',
      adminEmail: '',
      adminPassword: '',
      confirmAdminPassword: '',
    },
  })

  const onSubmit = (data: AssociationFormValues) => {
    setApiError(null)
    const payload = {
      name: data.name,
      slug: data.slug,
      logo: data.logo,
      street: data.street,
      number: data.number,
      neighborhood: data.neighborhood,
      city: data.city,
      state: data.state,
      zipCode: data.zipCode,
      adminEmail: data.adminEmail,
      adminPassword: data.adminPassword,
    }

    createAssociation(payload, {
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
            <label htmlFor="association-logo" className="mb-1 block text-sm font-medium text-gray-700">
              Logo da associação
            </label>
            <input
              id="association-logo"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              {...register('logo')}
              className="block w-full text-sm text-gray-700 file:mr-4 file:rounded-lg file:border-0 file:bg-indigo-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-indigo-700 hover:file:bg-indigo-100"
              disabled={isPending}
            />
            {errors.logo && <p className="mt-1 text-xs text-red-600">{errors.logo.message as string}</p>}
          </div>

          <div>
            <label htmlFor="association-street" className="mb-1 block text-sm font-medium text-gray-700">
              Rua
            </label>
            <input
              id="association-street"
              type="text"
              {...register('street')}
              placeholder="Ex.: Rua das Palmeiras"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.street && <p className="mt-1 text-xs text-red-600">{errors.street.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="association-number" className="mb-1 block text-sm font-medium text-gray-700">
                Número
              </label>
              <input
                id="association-number"
                type="text"
                {...register('number')}
                placeholder="123"
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
              {errors.number && <p className="mt-1 text-xs text-red-600">{errors.number.message}</p>}
            </div>

            <div>
              <label htmlFor="association-neighborhood" className="mb-1 block text-sm font-medium text-gray-700">
                Bairro
              </label>
              <input
                id="association-neighborhood"
                type="text"
                {...register('neighborhood')}
                placeholder="Centro"
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
              {errors.neighborhood && <p className="mt-1 text-xs text-red-600">{errors.neighborhood.message}</p>}
            </div>
          </div>

          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label htmlFor="association-city" className="mb-1 block text-sm font-medium text-gray-700">
                Cidade
              </label>
              <input
                id="association-city"
                type="text"
                {...register('city')}
                placeholder="São Paulo"
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
              {errors.city && <p className="mt-1 text-xs text-red-600">{errors.city.message}</p>}
            </div>

            <div>
              <label htmlFor="association-state" className="mb-1 block text-sm font-medium text-gray-700">
                Estado
              </label>
              <input
                id="association-state"
                type="text"
                {...register('state')}
                placeholder="SP"
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
              {errors.state && <p className="mt-1 text-xs text-red-600">{errors.state.message}</p>}
            </div>
          </div>

          <div>
            <label htmlFor="association-zip-code" className="mb-1 block text-sm font-medium text-gray-700">
              CEP
            </label>
            <input
              id="association-zip-code"
              type="text"
              {...register('zipCode')}
              placeholder="01000-000"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.zipCode && <p className="mt-1 text-xs text-red-600">{errors.zipCode.message}</p>}
          </div>

          <div>
            <label htmlFor="association-admin-email" className="mb-1 block text-sm font-medium text-gray-700">
              Email do admin inicial
            </label>
            <input
              id="association-admin-email"
              type="email"
              {...register('adminEmail')}
              placeholder="Ex.: admin@associacao.com"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.adminEmail && <p className="mt-1 text-xs text-red-600">{errors.adminEmail.message}</p>}
          </div>

          <div>
            <label htmlFor="association-admin-password" className="mb-1 block text-sm font-medium text-gray-700">
              Senha do admin inicial
            </label>
            <input
              id="association-admin-password"
              type="password"
              {...register('adminPassword')}
              placeholder="Mínimo 8 caracteres"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.adminPassword && <p className="mt-1 text-xs text-red-600">{errors.adminPassword.message}</p>}
          </div>

          <div>
            <label htmlFor="association-admin-password-confirm" className="mb-1 block text-sm font-medium text-gray-700">
              Confirmar senha do admin
            </label>
            <input
              id="association-admin-password-confirm"
              type="password"
              {...register('confirmAdminPassword')}
              placeholder="Repita a senha"
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
            {errors.confirmAdminPassword && (
              <p className="mt-1 text-xs text-red-600">{errors.confirmAdminPassword.message}</p>
            )}
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
