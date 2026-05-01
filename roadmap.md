# 🚀 Roadmap de Implementação — BabaPlay SaaS Associações Esportivas

## 🎯 Objetivo

Construir um sistema SaaS escalável, com:

- Multi-tenancy (1 DB por associação)
- TDD obrigatório
- CQRS obrigatório
- ASP.NET Identity
- Tempo real (SignalR)
- Notificações (Firebase)

---

## 🧱 Fase 0 — Fundação ✅ CONCLUÍDA

### Entregas

- Estrutura:
  - Api
  - Application
  - Domain
  - Infrastructure
  - Tests

- Configuração:
  - Clean Architecture
  - CQRS
  - xUnit + FluentAssertions
  - Swagger

### Testes (7 total — 100% passando)

- Unitários: `PingCommandHandler`, `PingQueryHandler`, `EntityBase`
- Integração: `PingIntegrationTests`

---

## 🔐 Fase 1 — Identity + Auth ✅ CONCLUÍDA

### Entregas

- ASP.NET Identity (`ApplicationUser : IdentityUser` + `IsActive`)
- JWT com HmacSha256 (claims: sub, email, jti, iat, role); ClockSkew = Zero
- Refresh Token persistido no DB, rotacionado a cada uso, revogável
- Master DB: Tenants, UserTenants, Plans, Subscriptions (SaaS full)
- EF Core migration: `InitialMaster`
- `POST /api/v1/auth/login` — retorna `AuthResponse` (accessToken + refreshToken)
- `POST /api/v1/auth/refresh-token` — rotação de token
- Swagger com Bearer JWT security definition

### Testes (27 total — 100% passando)

- 10 testes unitários (LoginCommandHandler × 5, RefreshTokenCommandHandler × 5)
- 7 testes de integração (login válido, inválido, email desconhecido, refresh válido, expirado, inválido, token revogado)

---

## 🏢 Fase 2 — Multi-Tenancy

### Entregas

- Tenants
- UserTenants
- Middleware de tenant
- DbContext dinâmico
- Provisionamento automático de banco

### Testes

- Isolamento de dados
- Troca de tenant

---

## 👤 Fase 3 — Players

### Entregas

- Player (perfil do usuário)
- CRUD completo

### Testes

- Criação
- Atualização

---

## 🔐 Fase 4 — RBAC

### Entregas

- Roles
- Permissions
- Middleware de autorização

### Testes

- Permissão válida
- Permissão negada

---

## ⚽ Fase 5 — Posições

### Entregas

- Positions
- PlayerPositions

### Testes

- Limite de 3 posições

---

## 📅 Fase 6 — Dias de Jogo

### Entregas

- GameDays

---

## 📍 Fase 7 — Check-in

### Entregas

- Check-in com geolocalização
- Validação de raio
- SignalR

### Testes

- Dentro do raio
- Fora do raio
- Duplicidade

---

## 🧩 Fase 8 — Times

### Entregas

- Teams
- TeamPlayers

### Regras

- Limite de jogadores
- Goleiro obrigatório

---

## 🏆 Fase 9 — Partidas

### Entregas

- Matches
- Status

---

## 📊 Fase 10 — Eventos

### Entregas

- MatchEvents:
  - Gol
  - Amarelo
  - Vermelho

---

## 📄 Fase 11 — Súmula

### Entregas

- Geração de PDF
- Armazenamento

---

## 🧠 Fase 12 — Score e Ranking

### Regras

- Presença: +1
- Vitória: +3
- Empate: +1
- Gol: +2
- Amarelo: -1
- Vermelho: -3

### Entregas

- Ranking geral
- Artilharia
- Presença

---

## 🔔 Fase 13 — Notificações

### Entregas

- Firebase
- Push por evento

---

## 💰 Fase 14 — Financeiro

### Entregas

- Entradas e saídas
- Mensalidades

### Relatórios

- Caixa
- Inadimplência

---

## 📊 Fase 15 — Relatórios

### Entregas

- Presença
- Ranking
- Financeiro

---

## 🌐 Fase 16 — Frontend (React)

### 1. Auth ✅ CONCLUÍDA

#### Stack
- React 19 + Vite 8 + TypeScript 6
- TailwindCSS 4
- Zustand + persist (auth state)
- TanStack React Query v5
- TanStack Router (guards via `beforeLoad`)
- Axios + interceptor queue (silent refresh)
- Zod (validação de formulários)
- MSW 2 + Vitest + Testing Library (TDD)

#### Entregas
- `authService`: `login`, `refreshToken`, `logout` (revogação server-side), `getCurrentUser`
- `authStore` (Zustand): `accessToken`, `refreshToken`, `isAuthenticated`, `currentUser`; `setTokens`, `setCurrentUser`, `clearTokens` atômico
- `useLogin`: mutation com pré-fetch de perfil após login
- `useLogout`: mutation com `onSettled` — limpa store + cache + navega independente de erro
- `useCurrentUser`: query habilitada por `isAuthenticated`, popula `currentUser` no store
- `AuthHeader`: exibe email do usuário + botão Sair
- `ErrorBoundary`: captura erros de renderização, exibe fallback + reload
- `ProtectedLayout`: integra `AuthHeader` + aciona `useCurrentUser`
- Router guards: redirecionamento automático autenticado/não-autenticado
- Contrato de erros: `ProblemDetails.title` → `getErrorCode()`

#### Testes — 52 total (100% passando)
| Suíte | Testes |
|---|---|
| authStore | 6 |
| loginSchema | 5 |
| apiClient (interceptors) | 4 |
| authService | 10 |
| useLogin | 5 |
| useLogout | 3 |
| useCurrentUser | 4 |
| LoginForm | 7 |
| AuthHeader | 4 |
| ErrorBoundary | 4 |

### 2. Dashboard — pendente
### 3. Players — pendente
### 4. Check-in — pendente
### 5. Times — pendente
### 6. Partidas — pendente
### 7. Ranking — pendente
### 8. Financeiro — pendente

---

## 📱 Fase 17 — Mobile (Flutter)

### Ordem

1. Login
2. Check-in
3. Notificações
4. Jogos
5. Ranking

---

## ⚠️ Regras obrigatórias

- TDD sempre
- CQRS sempre
- Documentação sempre atualizada
- Testes obrigatórios (mínimo 80%)
- Falha de teste bloqueia deploy

---

## 🧠 Estratégia

- Fases críticas: 1 semana
- Fases simples: 2-3 dias

### MVP

Parar na Fase 11 já gera valor real.

---

Fim do documento.
