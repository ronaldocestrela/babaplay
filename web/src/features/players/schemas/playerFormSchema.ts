import { z } from 'zod'

export const playerFormSchema = z.object({
  userId: z.string().uuid('UserId inválido').optional(),
  name: z
    .string()
    .trim()
    .min(1, 'Nome é obrigatório')
    .max(120, 'Nome deve ter no máximo 120 caracteres'),
  nickname: z.string().trim().max(60, 'Apelido deve ter no máximo 60 caracteres').optional(),
  phone: z.string().trim().max(30, 'Telefone deve ter no máximo 30 caracteres').optional(),
  dateOfBirth: z
    .string()
    .trim()
    .refine((value) => value.length === 0 || /^\d{4}-\d{2}-\d{2}$/.test(value), {
      message: 'Data de nascimento inválida',
    })
    .optional(),
  positionIds: z.array(z.string().uuid('Posição inválida')).max(3, 'Máximo de 3 posições por jogador'),
})

export type PlayerFormValues = z.infer<typeof playerFormSchema>