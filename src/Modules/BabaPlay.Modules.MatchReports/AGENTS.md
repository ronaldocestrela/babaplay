# AGENTS — BabaPlay.Modules.MatchReports

## Domínio

**MatchReport**, **MatchReportGame**, **MatchReportPlayerStat**.

## Regras

- Uma sessão pode ter várias partidas na mesma súmula.
- Estatísticas por jogador usam associados do tenant.
- Gols, assistências e cartões devem ser valores não negativos.
- Súmula finalizada só pode ser editada por `Admin`.

## Serviços

- `MatchReportService` — consulta, criação/edição e finalização da súmula por sessão.

## Controllers

- `MatchReportsController` — `/api/matchreports/sessions/{sessionId}` e ação de finalização.