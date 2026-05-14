import { describe, it, expect } from 'vitest'
import { loginSchema } from '../loginSchema'

describe('loginSchema', () => {
  it('deve validar corretamente com email e senha válidos', () => {
    const result = loginSchema.safeParse({ email: 'test@example.com', password: '123456' })
    expect(result.success).toBe(true)
  })

  it('deve rejeitar email inválido', () => {
    const result = loginSchema.safeParse({ email: 'nao-e-email', password: '123456' })
    expect(result.success).toBe(false)
    if (!result.success) {
      const emailError = result.error.issues.find((i) => i.path.includes('email'))
      expect(emailError).toBeDefined()
    }
  })

  it('deve rejeitar email vazio', () => {
    const result = loginSchema.safeParse({ email: '', password: '123456' })
    expect(result.success).toBe(false)
  })

  it('deve rejeitar senha com menos de 6 caracteres', () => {
    const result = loginSchema.safeParse({ email: 'test@example.com', password: '12345' })
    expect(result.success).toBe(false)
    if (!result.success) {
      const passwordError = result.error.issues.find((i) => i.path.includes('password'))
      expect(passwordError?.message).toBe('Senha deve ter no mínimo 6 caracteres')
    }
  })

  it('deve rejeitar senha vazia', () => {
    const result = loginSchema.safeParse({ email: 'test@example.com', password: '' })
    expect(result.success).toBe(false)
  })
})
