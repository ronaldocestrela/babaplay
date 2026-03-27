# AGENTS — BabaPlay.Modules.Financial

## Domínio

**Category**, **CashEntry**, **Membership**, **Payment** (`MembershipStatus`).

## Serviços

- `CategoryService`, `CashEntryService` — caixa e categorias.
- `MembershipService` — mensalidades por associado/período, registo de pagamento (atualiza estado para `Paid`).

## Controllers

- `CategoriesController`, `CashEntriesController`, `MembershipsController` — rotas `/api/categories`, `/api/cashentries`, `/api/memberships`.

## Notas

- Índice único lógico `(AssociateId, Year, Month)` em `Membership` configurado no `TenantDbContext`.
