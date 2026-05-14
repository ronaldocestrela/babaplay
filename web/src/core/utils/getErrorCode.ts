import type { AxiosError } from 'axios'
import type { ProblemDetails } from '@/core/types/api'

/**
 * Extrai o código de erro (ProblemDetails.title) de um erro Axios.
 * Retorna null se o erro não for um AxiosError com payload ProblemDetails.
 */
export function getErrorCode(err: unknown): string | null {
  const axiosError = err as AxiosError<ProblemDetails>
  return axiosError?.response?.data?.title ?? null
}
