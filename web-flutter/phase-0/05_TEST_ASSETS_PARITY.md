# Fase 0 - Test Assets for Parity

## Objetivo
Mapear ativos de teste do React que precisam ser espelhados no Flutter para manter paridade.

## Fontes atuais
- Handlers de mock: [../../web/src/test/handlers.ts](../../web/src/test/handlers.ts)
- Server setup: [../../web/src/test/server.ts](../../web/src/test/server.ts)
- Test setup: [../../web/src/test/setup.ts](../../web/src/test/setup.ts)
- Cobertura e thresholds: [../../web/vite.config.ts](../../web/vite.config.ts)

## Itens para espelhar no Flutter
1. Fixtures de autenticacao.
- AuthResponse, UserProfile, tenants.

2. Fixtures de dominio.
- Players, positions, teams, matches, checkins, gamedays, tenant settings.

3. Catalogo de erro.
- ProblemDetails.title mapeado para erros de dominio/UI no Flutter.

4. Casos de regressao critica.
- Auth refresh concorrente.
- Guards de onboarding/tenant.
- Fluxo de checkin com geolocalizacao.

## Estrutura sugerida para fixtures Flutter
- `test/fixtures/auth/`
- `test/fixtures/players/`
- `test/fixtures/teams/`
- `test/fixtures/matches/`
- `test/fixtures/checkins/`
- `test/fixtures/tenant/`

## Metas minimas de teste na migracao
- Unit tests para use cases e view models.
- Widget tests para telas criticas.
- Golden tests para componentes e telas chave.
- Testes de navegacao para guardas.

## Checklist
- [ ] Mapeamento MSW -> fixtures Flutter completo.
- [ ] Lista de cenarios obrigatorios de regressao aprovada.
- [ ] Meta de cobertura para Flutter definida e publicada.
