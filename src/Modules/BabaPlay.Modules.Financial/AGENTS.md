# AGENTS — BabaPlay.Modules.Financial

## Domínio

**Category**, **CashEntry**, **Membership**, **Payment** (`MembershipStatus`).

- `Category` possui `CategoryType` (`Income`/`Expense`).
- `CashEntry` possui `CurrentBalance` (saldo acumulado após cada movimento).

## Serviços

- `CategoryService`, `CashEntryService` — caixa e categorias.
- `MembershipService` — mensalidades por associado/período, registo de pagamento (atualiza estado para `Paid` e cria movimento de caixa de receita).

## Controllers

- `CategoriesController`, `CashEntriesController`, `MembershipsController` — rotas `/api/categories`, `/api/cashentries`, `/api/memberships`.

## Notas

- Índice único lógico `(AssociateId, Year, Month)` em `Membership` configurado no `TenantDbContext`.
- Regra de cálculo do saldo: usa sempre `abs(Amount)`; categorias `Income` somam e `Expense` subtraem.
- Pagamentos de mensalidade usam/criam a categoria `Pagamento de mensalidade` (tipo `Income`) para impactar o caixa.
