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
})

export type AssociationFormValues = z.infer<typeof associationFormSchema>
