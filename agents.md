# AGENTS.md

## Visão Geral

BabaPlay Sistema SaaS para gestão de associações esportivas (futebol) com:

- Multi-tenant (1 banco por associação)
- Tempo real (SignalR)
- Notificações push (Firebase)
- Backend: .NET (Clean Architecture + CQRS + TDD obrigatório)
- Banco: SQL Server
- Frontend: React + Vite + TypeScript
- Mobile: Flutter

Escala alvo inicial:
- ~500 tenants
- ~60 usuários por tenant

---

## PRINCÍPIOS OBRIGATÓRIOS (NÃO NEGOCIÁVEL)

### 1. TDD (Test Driven Development)

- Todo código deve ser iniciado por teste
- Não é permitido código sem teste
- Fluxo obrigatório:
  1. Red (teste falha)
  2. Green (implementação mínima)
  3. Refactor

### 2. CQRS

Separação obrigatória:

- Commands → escrita
- Queries → leitura

NUNCA misturar responsabilidades.

### 3. Clean Architecture

- Domain não depende de nada
- Application depende apenas de Domain
- Infrastructure depende de tudo
- API apenas orquestra

### 4. Identity

Uso obrigatório do ASP.NET Identity para:

- Autenticação
- Gestão de usuários
- Roles base

Customizações devem respeitar o Identity.

### 5. Documentação Atualizada

- Toda feature deve atualizar documentação
- API deve ser documentada (Swagger obrigatório)
- Mudança de regra → atualização obrigatória no AGENTS.md

---

## Arquitetura Geral

### Master Database

Responsável por:

- Autenticação global (Identity)
- Gestão de tenants
- Assinaturas (SaaS)
- Vínculo usuário ↔ associação

#### Tabelas

- AspNetUsers
- AspNetRoles
- Tenants
- UserTenants
- Subscriptions
- Plans

---

### Tenant Database (1 por associação)

Responsável por:

- Jogadores
- Check-ins
- Partidas
- Times
- Eventos de jogo
- Financeiro
- RBAC

---

## Backend (.NET)

### Stack

- .NET 8
- ASP.NET Core
- Entity Framework Core
- SQL Server
- SignalR
- Firebase Admin SDK

### Estrutura

```
src/
 ├── Api
 ├── Application
 │    ├── Commands
 │    ├── Queries
 │    ├── Handlers
 │    ├── DTOs
 ├── Domain
 ├── Infrastructure
 ├── Tests
```

---

## Multi-Tenancy

### Estratégia

- 1 banco por tenant
- Connection string dinâmica

### Componentes obrigatórios

- ITenantResolver
- TenantMiddleware
- TenantDbContextFactory

---

## Tempo Real (SignalR)

### Hubs

- CheckinHub
- MatchHub

---

## Notificações (Firebase)

### Casos

- Check-in aberto
- Início de jogo
- Resultado

---

## Modelagem (Resumo)

### Master DB

- Users (Identity)
- Tenants
- UserTenants

### Tenant DB

- Players
- Positions
- Matches
- Teams
- MatchEvents
- Checkins
- Financial

---

## API

### Padrão

- Controllers leves
- Toda lógica via Handlers (CQRS)

Exemplo:

```
POST /matches
→ CreateMatchCommand
```

---

## Frontend (React)

- React + TS
- React Query
- Zustand

---

## Mobile (Flutter)

- Check-in GPS
- Push notifications

---

## Testes

Obrigatórios:

- Unitários (Domain + Application)
- Integração (API)

Cobertura mínima:

- 80%

---

## DevOps

- Docker
- CI/CD obrigatório com testes
- Falha de teste bloqueia deploy

---

## Regras de Negócio

### Score

- Presença: +1
- Vitória: +3
- Empate: +1
- Gol: +2
- Amarelo: -1
- Vermelho: -3

---

## Roadmap

1. Auth + Identity
2. Multi-tenancy
3. Players
4. Check-in
5. Matches
6. Score
7. Notificações
8. Financeiro

---

## REGRA FINAL

Qualquer código que:

- não tenha teste
- não siga CQRS
- não respeite Clean Architecture
- não atualize documentação

→ DEVE ser considerado inválido

---

Fim.

