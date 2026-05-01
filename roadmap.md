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

### Ordem

1. Auth
2. Dashboard
3. Players
4. Check-in
5. Times
6. Partidas
7. Ranking
8. Financeiro

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
