import { useEffect, useState } from 'react'
import { AssociationLocationMap } from '@/core/components/AssociationLocationMap'
import { useAuthStore } from '@/features/auth/store/authStore'
import { isTenantAdmin } from '@/features/auth/utils/tenantAccess'
import {
  useChangeTenantGameDayOptionStatus,
  useCreateTenantGameDayOption,
  useTenantGameDayOptions,
} from '@/features/tenant-settings/hooks/useTenantGameDayOptions'
import { useTenantSettings, useUpdateTenantSettings } from '@/features/tenant-settings/hooks/useTenantSettings'
import { ERROR_CODES } from '@/core/constants/errorCodes'
import { geocodeAddress, lookupAddressByZipCode } from '@/core/services/addressLookup'

const ERROR_MESSAGES: Record<string, string> = {
  [ERROR_CODES.TENANT_NAME_REQUIRED]: 'Nome da associação é obrigatório.',
  [ERROR_CODES.TENANT_STREET_REQUIRED]: 'Rua é obrigatória.',
  [ERROR_CODES.TENANT_NUMBER_REQUIRED]: 'Número é obrigatório.',
  [ERROR_CODES.TENANT_CITY_REQUIRED]: 'Cidade é obrigatória.',
  [ERROR_CODES.TENANT_STATE_REQUIRED]: 'Estado é obrigatório.',
  [ERROR_CODES.TENANT_ZIPCODE_REQUIRED]: 'CEP é obrigatório.',
  [ERROR_CODES.TENANT_PLAYERS_PER_TEAM_INVALID]: 'Jogadores por time deve ser maior que zero.',
  [ERROR_CODES.TENANT_LOGO_INVALID_TYPE]: 'Logo deve ser PNG, JPG ou WEBP.',
  [ERROR_CODES.TENANT_LOGO_INVALID_SIZE]: 'Logo deve ter até 2MB.',
  TENANT_ASSOCIATION_LATITUDE_INVALID: 'Latitude da associação inválida.',
  TENANT_ASSOCIATION_LONGITUDE_INVALID: 'Longitude da associação inválida.',
  [ERROR_CODES.TENANT_GAMEDAY_OPTION_ALREADY_EXISTS]: 'Já existe uma opção ativa para este dia e horário.',
  [ERROR_CODES.TENANT_GAMEDAY_OPTION_NOT_FOUND]: 'Opção de dia de jogo não encontrada.',
  [ERROR_CODES.FORBIDDEN]: 'Somente admin pode editar as opções da associação.',
}

const GAME_DAY_LABELS: Record<number, string> = {
  0: 'Domingo',
  1: 'Segunda',
  2: 'Terça',
  3: 'Quarta',
  4: 'Quinta',
  5: 'Sexta',
  6: 'Sábado',
}

function normalizeTimeForApi(timeValue: string): string {
  if (timeValue.length === 5) {
    return `${timeValue}:00`
  }

  return timeValue
}

function formatTime(timeValue: string): string {
  const [hours, minutes] = timeValue.split(':')
  return `${hours ?? '00'}:${minutes ?? '00'}`
}

function isValidCoordinate(value: number, min: number, max: number): boolean {
  return Number.isFinite(value) && value >= min && value <= max
}

export function TenantSettingsPage() {
  const currentUser = useAuthStore((s) => s.currentUser)
  const currentTenant = useAuthStore((s) => s.currentTenant)
  const canEdit = isTenantAdmin(currentUser, currentTenant)

  const { data, isLoading } = useTenantSettings()
  const { updateSettings, isPending, errorCode } = useUpdateTenantSettings()
  const { data: gameDayOptions, isLoading: isGameDayOptionsLoading } = useTenantGameDayOptions()
  const {
    createOption,
    isPending: isCreatingOption,
    errorCode: createOptionErrorCode,
  } = useCreateTenantGameDayOption()
  const {
    changeStatus,
    isPending: isChangingOptionStatus,
    errorCode: changeOptionStatusErrorCode,
  } = useChangeTenantGameDayOptionStatus()

  const [name, setName] = useState('')
  const [playersPerTeam, setPlayersPerTeam] = useState(11)
  const [street, setStreet] = useState('')
  const [number, setNumber] = useState('')
  const [neighborhood, setNeighborhood] = useState('')
  const [city, setCity] = useState('')
  const [state, setState] = useState('')
  const [zipCode, setZipCode] = useState('')
  const [associationLatitude, setAssociationLatitude] = useState('')
  const [associationLongitude, setAssociationLongitude] = useState('')
  const [logo, setLogo] = useState<File | undefined>(undefined)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [zipLookupError, setZipLookupError] = useState<string | null>(null)
  const [locationLookupError, setLocationLookupError] = useState<string | null>(null)
  const [zipLookupSuccess, setZipLookupSuccess] = useState<string | null>(null)
  const [locationLookupSuccess, setLocationLookupSuccess] = useState<string | null>(null)
  const [isZipLookupPending, setIsZipLookupPending] = useState(false)
  const [isGeocodingPending, setIsGeocodingPending] = useState(false)
  const [newOptionDayOfWeek, setNewOptionDayOfWeek] = useState(2)
  const [newOptionStartTime, setNewOptionStartTime] = useState('20:00')
  const [gameDayOptionSuccessMessage, setGameDayOptionSuccessMessage] = useState<string | null>(null)

  const gameDayOptionErrorCode = createOptionErrorCode ?? changeOptionStatusErrorCode

  useEffect(() => {
    if (!data) return

    setName(data.name)
    setPlayersPerTeam(data.playersPerTeam)
    setStreet(data.street ?? '')
    setNumber(data.number ?? '')
    setNeighborhood(data.neighborhood ?? '')
    setCity(data.city ?? '')
    setState(data.state ?? '')
    setZipCode(data.zipCode ?? '')
    setAssociationLatitude(data.associationLatitude?.toFixed(6) ?? '')
    setAssociationLongitude(data.associationLongitude?.toFixed(6) ?? '')
  }, [data])

  const resolveCoordinatesFromCurrentAddress = async (values?: {
    street?: string
    neighborhood?: string
    city?: string
    state?: string
  }) => {
    const resolvedStreet = values?.street ?? street
    const resolvedNeighborhood = values?.neighborhood ?? neighborhood
    const resolvedCity = values?.city ?? city
    const resolvedState = values?.state ?? state

    if (!resolvedStreet.trim() || !resolvedCity.trim() || !resolvedState.trim()) {
      return
    }

    setLocationLookupError(null)
    setLocationLookupSuccess(null)
    setIsGeocodingPending(true)

    try {
      const geocoded = await geocodeAddress(
        {
          zipCode,
          street: resolvedStreet,
          number,
          neighborhood: resolvedNeighborhood,
          city: resolvedCity,
          state: resolvedState,
        },
      )

      if (!geocoded) {
        setLocationLookupError('Não foi possível localizar esse endereço no mapa. Ajuste latitude/longitude manualmente.')
        return
      }

      setAssociationLatitude(geocoded.latitude.toFixed(6))
      setAssociationLongitude(geocoded.longitude.toFixed(6))
      setLocationLookupSuccess('Localização da associação atualizada automaticamente no mapa.')
    } catch {
      setLocationLookupError('Falha ao obter latitude e longitude do endereço informado.')
    } finally {
      setIsGeocodingPending(false)
    }
  }

  const handleZipCodeLookup = async () => {
    const normalizedZipCode = zipCode.replace(/\D/g, '')
    if (normalizedZipCode.length !== 8) {
      return
    }

    setZipLookupError(null)
    setZipLookupSuccess(null)
    setIsZipLookupPending(true)

    try {
      const address = await lookupAddressByZipCode(zipCode)
      if (!address) {
        setZipLookupError('CEP não encontrado. Confira o CEP e preencha o endereço manualmente.')
        return
      }

      setStreet(address.street)
      setNeighborhood(address.neighborhood)
      setCity(address.city)
      setState(address.state)
      setZipLookupSuccess('Endereço preenchido automaticamente com base no CEP.')

      await resolveCoordinatesFromCurrentAddress({
        street: address.street,
        neighborhood: address.neighborhood,
        city: address.city,
        state: address.state,
      })
    } catch {
      setZipLookupError('Falha ao consultar o CEP. Tente novamente em instantes.')
    } finally {
      setIsZipLookupPending(false)
    }
  }

  if (!canEdit) {
    return (
      <main className="max-w-3xl mx-auto px-4 py-6">
        <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-amber-900">
          Somente admin da associação pode editar as opções do tenant.
        </div>
      </main>
    )
  }

  if (isLoading) {
    return (
      <main className="max-w-3xl mx-auto px-4 py-6">
        <p className="text-sm text-gray-600">Carregando configurações do tenant...</p>
      </main>
    )
  }

  const handleSubmit: React.FormEventHandler<HTMLFormElement> = (event) => {
    event.preventDefault()
    setSuccessMessage(null)

    const parsedLatitude = Number(associationLatitude)
    const parsedLongitude = Number(associationLongitude)

    if (!isValidCoordinate(parsedLatitude, -90, 90)) {
      setLocationLookupError('Latitude inválida. Informe um valor entre -90 e 90.')
      return
    }

    if (!isValidCoordinate(parsedLongitude, -180, 180)) {
      setLocationLookupError('Longitude inválida. Informe um valor entre -180 e 180.')
      return
    }

    updateSettings(
      {
        name,
        playersPerTeam,
        logo,
        street,
        number,
        neighborhood,
        city,
        state,
        zipCode,
        associationLatitude: parsedLatitude,
        associationLongitude: parsedLongitude,
      },
      {
        onSuccess: () => {
          setLogo(undefined)
          setSuccessMessage('Configurações da associação atualizadas com sucesso.')
        },
      },
    )
  }

  return (
    <main className="max-w-3xl mx-auto px-4 py-6">
      <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-semibold text-gray-900">Opções da Associação</h1>
        <p className="mt-1 text-sm text-gray-600">
          Atualize os dados da associação. Apenas administradores podem editar.
        </p>

        {successMessage && (
          <div className="mt-4 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
            {successMessage}
          </div>
        )}

        {errorCode && (
          <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
            {ERROR_MESSAGES[errorCode] ?? 'Falha ao atualizar as configurações da associação.'}
          </div>
        )}

        <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
          <div>
            <label htmlFor="tenant-name" className="mb-1 block text-sm font-medium text-gray-700">
              Nome da associação
            </label>
            <input
              id="tenant-name"
              type="text"
              value={name}
              onChange={(event) => setName(event.target.value)}
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
          </div>

          <div>
            <label htmlFor="tenant-players-per-team" className="mb-1 block text-sm font-medium text-gray-700">
              Jogadores por time
            </label>
            <input
              id="tenant-players-per-team"
              type="number"
              min={1}
              value={playersPerTeam}
              onChange={(event) => setPlayersPerTeam(Number(event.target.value) || 0)}
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
          </div>

          <div className="rounded-lg border border-gray-200 p-4 space-y-3">
            <div>
              <h2 className="text-base font-semibold text-gray-900">Dias de jogo recorrentes</h2>
              <p className="text-sm text-gray-600">Defina os dias e horários padrão para a associação.</p>
            </div>

            {gameDayOptionSuccessMessage && (
              <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
                {gameDayOptionSuccessMessage}
              </div>
            )}

            {gameDayOptionErrorCode && (
              <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                {ERROR_MESSAGES[gameDayOptionErrorCode] ?? 'Falha ao atualizar opções de dia de jogo.'}
              </div>
            )}

            <div className="grid grid-cols-1 gap-3 md:grid-cols-3 md:items-end">
              <div>
                <label htmlFor="tenant-gameday-day" className="mb-1 block text-sm font-medium text-gray-700">
                  Dia da semana
                </label>
                <select
                  id="tenant-gameday-day"
                  value={newOptionDayOfWeek}
                  onChange={(event) => setNewOptionDayOfWeek(Number(event.target.value))}
                  className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                  disabled={isPending || isCreatingOption || isChangingOptionStatus}
                >
                  {Object.entries(GAME_DAY_LABELS).map(([key, label]) => (
                    <option key={key} value={Number(key)}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label htmlFor="tenant-gameday-time" className="mb-1 block text-sm font-medium text-gray-700">
                  Horário
                </label>
                <input
                  id="tenant-gameday-time"
                  type="time"
                  value={newOptionStartTime}
                  onChange={(event) => setNewOptionStartTime(event.target.value)}
                  className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                  disabled={isPending || isCreatingOption || isChangingOptionStatus}
                />
              </div>

              <button
                type="button"
                className="h-11 rounded-lg border border-indigo-200 bg-indigo-50 px-4 text-sm font-semibold text-indigo-700 hover:bg-indigo-100 disabled:opacity-60"
                disabled={isPending || isCreatingOption || isChangingOptionStatus}
                onClick={() => {
                  setGameDayOptionSuccessMessage(null)
                  createOption(
                    {
                      dayOfWeek: newOptionDayOfWeek,
                      localStartTime: normalizeTimeForApi(newOptionStartTime),
                    },
                    {
                      onSuccess: () => {
                        setGameDayOptionSuccessMessage('Opção de dia de jogo adicionada com sucesso.')
                      },
                    },
                  )
                }}
              >
                {isCreatingOption ? 'Adicionando...' : 'Adicionar opção'}
              </button>
            </div>

            <div className="space-y-2">
              {isGameDayOptionsLoading ? (
                <p className="text-sm text-gray-600">Carregando opções de dia de jogo...</p>
              ) : gameDayOptions && gameDayOptions.length > 0 ? (
                gameDayOptions.map((option) => (
                  <div key={option.id} className="flex items-center justify-between rounded-lg border border-gray-200 bg-gray-50 px-3 py-2">
                    <div>
                      <p className="text-sm font-medium text-gray-900">
                        {GAME_DAY_LABELS[option.dayOfWeek] ?? `Dia ${option.dayOfWeek}`} às {formatTime(option.localStartTime)}
                      </p>
                      <p className={`text-xs ${option.isActive ? 'text-emerald-700' : 'text-gray-500'}`}>
                        {option.isActive ? 'Ativo' : 'Inativo'}
                      </p>
                    </div>
                    <button
                      type="button"
                      className="h-9 rounded-lg border border-gray-300 bg-white px-3 text-xs font-semibold text-gray-700 hover:bg-gray-100 disabled:opacity-60"
                      disabled={isPending || isCreatingOption || isChangingOptionStatus}
                      onClick={() => {
                        setGameDayOptionSuccessMessage(null)
                        changeStatus(
                          {
                            id: option.id,
                            isActive: !option.isActive,
                          },
                          {
                            onSuccess: () => {
                              setGameDayOptionSuccessMessage(
                                option.isActive
                                  ? 'Opção desativada com sucesso.'
                                  : 'Opção ativada com sucesso.',
                              )
                            },
                          },
                        )
                      }}
                    >
                      {option.isActive ? 'Desativar' : 'Ativar'}
                    </button>
                  </div>
                ))
              ) : (
                <p className="text-sm text-gray-600">Nenhuma opção recorrente cadastrada.</p>
              )}
            </div>
          </div>

          <div>
            <label htmlFor="tenant-logo" className="mb-1 block text-sm font-medium text-gray-700">
              Novo logo (opcional)
            </label>
            <input
              id="tenant-logo"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              onChange={(event) => {
                const file = event.target.files?.item(0)
                setLogo(file ?? undefined)
              }}
              className="block w-full text-sm text-gray-700 file:mr-4 file:rounded-lg file:border-0 file:bg-indigo-50 file:px-3 file:py-2 file:text-sm file:font-medium file:text-indigo-700 hover:file:bg-indigo-100"
              disabled={isPending}
            />
          </div>

          <div>
            <label htmlFor="tenant-street" className="mb-1 block text-sm font-medium text-gray-700">
              Rua
            </label>
            <input
              id="tenant-street"
              type="text"
              value={street}
              onChange={(event) => setStreet(event.target.value)}
              className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
              disabled={isPending}
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="tenant-number" className="mb-1 block text-sm font-medium text-gray-700">
                Número
              </label>
              <input
                id="tenant-number"
                type="text"
                value={number}
                onChange={(event) => setNumber(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>

            <div>
              <label htmlFor="tenant-neighborhood" className="mb-1 block text-sm font-medium text-gray-700">
                Bairro
              </label>
              <input
                id="tenant-neighborhood"
                type="text"
                value={neighborhood}
                onChange={(event) => setNeighborhood(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>
          </div>

          <div className="grid grid-cols-3 gap-3">
            <div className="col-span-2">
              <label htmlFor="tenant-city" className="mb-1 block text-sm font-medium text-gray-700">
                Cidade
              </label>
              <input
                id="tenant-city"
                type="text"
                value={city}
                onChange={(event) => setCity(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>

            <div>
              <label htmlFor="tenant-state" className="mb-1 block text-sm font-medium text-gray-700">
                Estado
              </label>
              <input
                id="tenant-state"
                type="text"
                value={state}
                onChange={(event) => setState(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>
          </div>

          <div>
            <label htmlFor="tenant-zip" className="mb-1 block text-sm font-medium text-gray-700">
              CEP
            </label>
            <div className="flex gap-2">
              <input
                id="tenant-zip"
                type="text"
                value={zipCode}
                onChange={(event) => setZipCode(event.target.value)}
                onBlur={handleZipCodeLookup}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending || isZipLookupPending}
              />
              <button
                type="button"
                onClick={handleZipCodeLookup}
                disabled={isPending || isZipLookupPending}
                className="h-11 shrink-0 rounded-lg border border-gray-300 px-4 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-60"
              >
                {isZipLookupPending ? 'Buscando...' : 'Buscar CEP'}
              </button>
            </div>
            {zipLookupError ? <p className="mt-1 text-xs text-red-600">{zipLookupError}</p> : null}
            {zipLookupSuccess ? <p className="mt-1 text-xs text-emerald-700">{zipLookupSuccess}</p> : null}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label htmlFor="tenant-association-latitude" className="mb-1 block text-sm font-medium text-gray-700">
                Latitude
              </label>
              <input
                id="tenant-association-latitude"
                type="text"
                value={associationLatitude}
                onChange={(event) => setAssociationLatitude(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>

            <div>
              <label htmlFor="tenant-association-longitude" className="mb-1 block text-sm font-medium text-gray-700">
                Longitude
              </label>
              <input
                id="tenant-association-longitude"
                type="text"
                value={associationLongitude}
                onChange={(event) => setAssociationLongitude(event.target.value)}
                className="h-11 w-full rounded-lg border border-gray-300 px-3 text-sm outline-none focus:border-indigo-500"
                disabled={isPending}
              />
            </div>
          </div>

          {(isGeocodingPending || locationLookupError || locationLookupSuccess) ? (
            <p className={`text-xs ${locationLookupError ? 'text-red-600' : locationLookupSuccess ? 'text-emerald-700' : 'text-gray-600'}`}>
              {isGeocodingPending
                ? 'Localizando coordenadas do endereço...'
                : (locationLookupError ?? locationLookupSuccess)}
            </p>
          ) : null}

          <AssociationLocationMap
            latitude={associationLatitude}
            longitude={associationLongitude}
            onCoordinateChange={(latitude, longitude) => {
              setAssociationLatitude(latitude)
              setAssociationLongitude(longitude)
            }}
          />

          <button
            type="submit"
            className="h-11 rounded-lg bg-indigo-600 px-5 text-sm font-semibold text-white transition-colors hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={isPending}
          >
            {isPending ? 'Salvando...' : 'Salvar alterações'}
          </button>
        </form>
      </section>
    </main>
  )
}
