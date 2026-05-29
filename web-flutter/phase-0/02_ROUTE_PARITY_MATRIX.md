# Fase 0 - Route Parity Matrix

## Instrucoes
Preencher status por rota e validar com frontend + backend antes do kickoff da Fase 1.

Legenda de status:
- `MAPPED`: mapeada no React e com regra definida para Flutter.
- `BLOCKED`: falta definicao de regra/estado/contrato.
- `DONE`: validada em revisao de arquitetura.

## Matriz
| Route | Tipo | Guard/Pre-condicao | Principal endpoint(s) | Flutter target | Status | Owner |
|---|---|---|---|---|---|---|
| /login | Publica | Se autenticado redireciona para / | /api/v1/auth/login | Auth/LoginPage | MAPPED | FE |
| /register-association | Publica | Sem auth obrigatoria | /api/v1/tenant | Onboarding/RegisterAssociationPage | MAPPED | FE |
| /invite/accept | Publica | Fluxo por token de convite | /api/v1/association-invite/* | Onboarding/AcceptInvitePage | MAPPED | FE |
| /select-tenant | Protegida | Auth obrigatoria, multiplo tenant sem selecao | /api/v1/auth/me | Auth/SelectTenantPage | MAPPED | FE |
| /players/complete-profile | Protegida | requiresPlayerOnboarding=true | /api/v1/player | Players/CompleteProfilePage | MAPPED | FE |
| / | Protegida | Auth + tenant selecionado quando necessario | /api/v1/ranking/* /api/v1/financial/* | Dashboard/HomePage | MAPPED | FE |
| /players | Protegida | Auth + tenant | /api/v1/player /api/v1/position | Players/PlayersPage | MAPPED | FE |
| /teams | Protegida | Auth + tenant | /api/v1/team /api/v1/player | Teams/TeamsPage | MAPPED | FE |
| /matches | Protegida | Auth + tenant | /api/v1/match /api/v1/team /api/v1/gameday | Matches/MatchesPage | MAPPED | FE |
| /positions | Protegida | Auth + tenant + regra de acesso | /api/v1/position | Positions/PositionsPage | MAPPED | FE |
| /checkins | Protegida | Auth + tenant + perfil valido | /api/v1/checkin/* | Checkins/CheckinsPage | MAPPED | FE |
| /tenant/settings | Protegida | Auth + tenant + role admin | /api/v1/tenant/settings* | Tenant/SettingsPage | MAPPED | FE |

## Regras transversais obrigatorias
- Auth guard equivalente ao React.
- Onboarding guard equivalente ao React.
- Tenant selection guard equivalente ao React.
- Header `X-Tenant-Slug` injetado automaticamente em requests protegidas.
- Refresh token com fila em concorrencia para 401.

## Checklist de validacao
- [ ] 100% das rotas mapeadas.
- [ ] Guardas revisadas por time.
- [ ] Endpoints confirmados com backend.
- [ ] Sem rota critica em status BLOCKED.
