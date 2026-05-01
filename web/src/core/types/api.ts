/** Espelha o formato ProblemDetails retornado pelo GlobalExceptionHandler do backend. */
export interface ProblemDetails {
  type?: string
  title: string
  status: number
  detail?: string
  instance?: string
}

/** Erro genérico de API com payload tipado. */
export interface ApiError {
  code: string
  message: string
}
