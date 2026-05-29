# Fase 0 - Critical Scenarios (Auth, Tenant, Checkin)

## Objetivo
Fixar cenarios de aceite para evitar regressao no que e mais sensivel na migracao BigBang.

## Auth + Session
1. Login sucesso.
- Esperado: token persistido, perfil carregado, redirecionamento correto.

2. Login invalido.
- Esperado: exibicao de codigo/erro correto sem quebrar fluxo.

3. Refresh token em concorrencia (multiplas requests 401).
- Esperado: uma renovacao, fila de reexecucao, sem logout indevido.

4. Refresh falha.
- Esperado: limpar sessao e redirecionar para login.

## Tenant Context
1. Usuario com 1 tenant.
- Esperado: tenant selecionado automaticamente.

2. Usuario com multiplos tenants sem selecao.
- Esperado: redirecionar para select-tenant antes de acessar rotas internas.

3. Mudanca de tenant.
- Esperado: persistencia correta e requests com novo `X-Tenant-Slug`.

## Onboarding Gate
1. Usuario requer onboarding de jogador.
- Esperado: bloqueio das rotas internas e redirecionamento para complete-profile.

2. Onboarding concluido.
- Esperado: desbloqueio de navegacao padrao.

## Checkin + Geo
1. Permissao de localizacao concedida.
- Esperado: coordenadas preenchidas e checkin normal.

2. Permissao negada.
- Esperado: fallback manual sem travar fluxo.

3. Fora do raio permitido.
- Esperado: erro de negocio correto.

## Criterios de aceite
- [ ] Todos os cenarios documentados com pre-condicao, acao e resultado esperado.
- [ ] Cenarios revisados por FE + BE + QA.
- [ ] Cenarios viram casos de teste da Fase 1/Fase 3.
