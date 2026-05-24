import { z } from 'zod'

export const manualPlayerFormSchema = z.object({
  email: z.string().trim().email('Informe um e-mail válido.'),
  password: z
    .string()
    .trim()
    .min(8, 'Senha temporária deve ter no mínimo 8 caracteres.'),
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
  positionIds: z
    .array(z.string())
    .max(3, 'Máximo de 3 posições por jogador.'),
})

export type ManualPlayerFormValues = z.infer<typeof manualPlayerFormSchema>
