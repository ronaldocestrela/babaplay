import { describe, expect, it } from 'vitest'
import { tenantGameDayOptionsService } from '../tenantGameDayOptionsService'

describe('tenantGameDayOptionsService', () => {
  it('deve listar opções de dia de jogo do tenant', async () => {
    const result = await tenantGameDayOptionsService.getOptions()

    expect(result.length).toBeGreaterThan(0)
    expect(result[0]?.id).toBeDefined()
    expect(result[0]?.localStartTime).toBeDefined()
  })

  it('deve criar opção de dia de jogo do tenant', async () => {
    const result = await tenantGameDayOptionsService.createOption({
      dayOfWeek: 1,
      localStartTime: '20:00:00',
    })

    expect(result.dayOfWeek).toBe(1)
    expect(result.localStartTime).toBe('20:00:00')
    expect(result.isActive).toBe(true)
  })

  it('deve alterar status da opção de dia de jogo do tenant', async () => {
    const created = await tenantGameDayOptionsService.createOption({
      dayOfWeek: 4,
      localStartTime: '19:30:00',
    })

    const result = await tenantGameDayOptionsService.changeStatus({
      id: created.id,
      isActive: false,
    })

    expect(result.id).toBe(created.id)
    expect(result.isActive).toBe(false)
  })
})
