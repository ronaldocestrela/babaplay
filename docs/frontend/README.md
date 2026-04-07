# Documentação de API para o frontend

Cada **controller** HTTP tem um ficheiro próprio nesta pasta, com rotas, **payloads** (JSON) e **respostas** no envelope padrão da API.

## Convenções globais

Ver [api-conventions.md](api-conventions.md).

Notas recentes:
- **Associações / times:** `POST /api/associations` aceita `playersPerTeam` (default 5); `POST /api/teams/generate` envia só `sessionId` — o número de equipas deriva desse valor e dos check-ins (ver [associations-controller.md](associations-controller.md), [teams-controller.md](teams-controller.md)).

Notas financeiras recentes:
- `categories` agora exige `type` (`0 = Income`, `1 = Expense`) no `POST`.
- `cashentries` agora retorna `currentBalance` e `category.type`.
- Pagamentos em `memberships/{id}/payments` geram movimento de receita no caixa.

## Índice por controller

| Ficheiro | Base path (resumo) |
|----------|-------------------|
| [auth-controller.md](auth-controller.md) | `/api/auth` |
| [roles-controller.md](roles-controller.md) | `/api/roles` |
| [permissions-controller.md](permissions-controller.md) | `/api/permissions` |
| [associations-controller.md](associations-controller.md) | `/api/associations` |
| [associates-controller.md](associates-controller.md) | `/api/associates` |
| [positions-controller.md](positions-controller.md) | `/api/positions` |
| [checkins-controller.md](checkins-controller.md) | `/api/checkins` |
| [teams-controller.md](teams-controller.md) | `/api/teams` |
| [memberships-controller.md](memberships-controller.md) | `/api/memberships` |
| [categories-controller.md](categories-controller.md) | `/api/categories` |
| [cashentries-controller.md](cashentries-controller.md) | `/api/CashEntries` |
| [plans-controller.md](plans-controller.md) | `/api/platform/plans` |
| [tenants-controller.md](tenants-controller.md) | `/api/platform/tenants` |
