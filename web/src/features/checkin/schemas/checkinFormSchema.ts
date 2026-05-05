import { z } from 'zod'

export const checkinFormSchema = z.object({
  playerId: z.string().uuid('Jogador inválido'),
  gameDayId: z.string().uuid('Dia de jogo inválido'),
  checkedInAtUtc: z.string().datetime('Data/hora do check-in inválida').optional(),
  latitude: z
    .number({ message: 'Latitude é obrigatória' })
    .min(-90, 'Latitude inválida')
    .max(90, 'Latitude inválida'),
  longitude: z
    .number({ message: 'Longitude é obrigatória' })
    .min(-180, 'Longitude inválida')
    .max(180, 'Longitude inválida'),
})

export type CheckinFormValues = z.infer<typeof checkinFormSchema>
