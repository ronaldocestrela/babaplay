import { describe, expect, it } from 'vitest'
import { teamFormSchema } from '../teamFormSchema'

describe('teamFormSchema', () => {
  it('deve validar dados válidos', () => {
    const result = teamFormSchema.safeParse({
      name: 'Time Azul',
      maxPlayers: 12,
    })

    expect(result.success).toBe(true)
  })

  it('deve invalidar nome vazio', () => {
    const result = teamFormSchema.safeParse({
      name: '   ',
      maxPlayers: 11,
    })

    expect(result.success).toBe(false)
  })

  it('deve invalidar maxPlayers menor ou igual a zero', () => {
    const result = teamFormSchema.safeParse({
      name: 'Time Laranja',
      maxPlayers: 0,
    })

    expect(result.success).toBe(false)
  })
})
