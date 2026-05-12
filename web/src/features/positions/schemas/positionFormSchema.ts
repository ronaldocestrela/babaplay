import { z } from 'zod'

export const positionFormSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, 'Código é obrigatório')
    .max(50, 'Código deve ter no máximo 50 caracteres'),
  name: z
    .string()
    .trim()
    .min(1, 'Nome é obrigatório')
    .max(100, 'Nome deve ter no máximo 100 caracteres'),
  description: z
    .string()
    .trim()
    .max(300, 'Descrição deve ter no máximo 300 caracteres')
    .optional(),
})