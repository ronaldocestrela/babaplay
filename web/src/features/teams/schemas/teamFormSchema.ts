import { z } from 'zod'

export const teamFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'Nome é obrigatório')
    .max(120, 'Nome deve ter no máximo 120 caracteres'),
  maxPlayers: z
    .number({ message: 'Máximo de jogadores é obrigatório' })
    .int('Máximo de jogadores deve ser inteiro')
    .positive('Máximo de jogadores deve ser maior que zero'),
})

export type TeamFormValues = z.infer<typeof teamFormSchema>
