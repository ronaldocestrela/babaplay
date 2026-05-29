# Fase 0 - Visual Baseline Checklist

## Objetivo
Capturar baseline visual oficial do sistema atual para garantir paridade no Flutter.

## Breakpoints obrigatorios
- Mobile: 375x812
- Tablet: 768x1024
- Desktop: 1366x768 (ou maior)

## Estados obrigatorios por tela
- Estado inicial
- Loading
- Sucesso com dados
- Empty state
- Erro de API
- Erro de permissao (quando aplicavel)

## Telas alvo
- Login
- Register Association
- Accept Invite
- Select Tenant
- Complete Profile
- Dashboard
- Players
- Teams
- Matches
- Positions
- Checkins
- Tenant Settings

## Tokens que devem ser validados
Fonte: [../../web/src/index.css](../../web/src/index.css)
- Cores de brand, surface, outline e error.
- Tipografia principal (Lexend/Inter).
- Border radius padrao.
- Espacamentos entre blocos.

## Evidencias
Salvar capturas em:
- `phase-0/artifacts/screenshots/mobile/`
- `phase-0/artifacts/screenshots/tablet/`
- `phase-0/artifacts/screenshots/desktop/`

## Checklist
- [ ] Todas as telas com 3 breakpoints capturados.
- [ ] Todos os estados obrigatorios capturados.
- [ ] Capturas revisadas por produto/design.
- [ ] Diferenças relevantes anotadas para Design System Flutter.
