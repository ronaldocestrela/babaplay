import { z } from 'zod'

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
