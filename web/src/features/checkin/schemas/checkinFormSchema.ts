import { z } from 'zod'

export const checkinFormSchema = z.object({
  playerId: z.string().trim().min(1, 'Jogador inválido'),
  gameDayId: z.string().trim().min(1, 'Dia de jogo inválido'),
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
