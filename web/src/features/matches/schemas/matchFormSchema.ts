import { z } from 'zod'

export const matchFormSchema = z
  .object({
    gameDayId: z.string().min(1, 'Dia de jogo é obrigatório'),
    homeTeamId: z.string().min(1, 'Time mandante é obrigatório'),
    awayTeamId: z.string().min(1, 'Time visitante é obrigatório'),
    description: z.string().trim().max(500, 'Descrição deve ter no máximo 500 caracteres').optional(),
  })
  .refine((data) => data.homeTeamId !== data.awayTeamId, {
    message: 'Times mandante e visitante devem ser diferentes',
    path: ['awayTeamId'],
  })

export type MatchFormValues = z.infer<typeof matchFormSchema>
