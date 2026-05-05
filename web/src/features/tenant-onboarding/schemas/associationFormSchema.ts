import { z } from 'zod'

const MAX_LOGO_BYTES = 2 * 1024 * 1024
const ALLOWED_LOGO_TYPES = ['image/png', 'image/jpeg', 'image/webp']

export const associationFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Nome da associação é obrigatório')
    .max(120, 'Nome deve ter no máximo 120 caracteres'),
  slug: z
    .string()
    .trim()
    .min(1, 'Slug é obrigatório')
    .max(80, 'Slug deve ter no máximo 80 caracteres')
    .regex(/^[a-z0-9-]+$/, 'Slug deve conter apenas letras minúsculas, números e hífens'),
  logo: z.preprocess(
    (value) => {
      if (typeof FileList !== 'undefined' && value instanceof FileList) {
        return value.item(0)
      }

      return value
    },
    z
      .instanceof(File, { message: 'Logo da associação é obrigatório' })
      .refine((file) => ALLOWED_LOGO_TYPES.includes(file.type), 'Logo deve ser PNG, JPG ou WEBP')
      .refine((file) => file.size > 0 && file.size <= MAX_LOGO_BYTES, 'Logo deve ter até 2MB'),
  ),
  street: z
    .string()
    .trim()
    .min(1, 'Rua é obrigatória')
    .max(160, 'Rua deve ter no máximo 160 caracteres'),
  number: z
    .string()
    .trim()
    .min(1, 'Número é obrigatório')
    .max(30, 'Número deve ter no máximo 30 caracteres'),
  neighborhood: z
    .string()
    .trim()
    .max(120, 'Bairro deve ter no máximo 120 caracteres')
    .optional(),
  city: z
    .string()
    .trim()
    .min(1, 'Cidade é obrigatória')
    .max(100, 'Cidade deve ter no máximo 100 caracteres'),
  state: z
    .string()
    .trim()
    .min(1, 'Estado é obrigatório')
    .max(100, 'Estado deve ter no máximo 100 caracteres'),
  zipCode: z
    .string()
    .trim()
    .min(1, 'CEP é obrigatório')
    .max(20, 'CEP deve ter no máximo 20 caracteres'),
  adminEmail: z
    .string()
    .trim()
    .min(1, 'Email do admin é obrigatório')
    .email('Email do admin inválido'),
  adminPassword: z
    .string()
    .min(8, 'Senha do admin deve ter no mínimo 8 caracteres')
    .regex(/[A-Z]/, 'Senha do admin deve conter ao menos uma letra maiúscula')
    .regex(/[0-9]/, 'Senha do admin deve conter ao menos um número'),
  confirmAdminPassword: z
    .string()
    .min(1, 'Confirmação de senha é obrigatória'),
}).refine((data) => data.adminPassword === data.confirmAdminPassword, {
  message: 'As senhas do admin devem ser iguais',
  path: ['confirmAdminPassword'],
})

export type AssociationFormValues = z.infer<typeof associationFormSchema>
