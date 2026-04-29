# AGENTS — BabaPlay.Modules.CheckIns

## Domínio

**CheckInSession**, **CheckIn**.

## Regras

- **Um check-in por dia e por associado** (UTC): verificação em `CheckInService.RegisterCheckInAsync`.
- **Uma sessão de check-in por dia por tenant** (UTC): verificação em `CheckInService.StartSessionAsync`. Tentativa de criar segunda sessão no mesmo dia UTC retorna **409 Conflict**. Protegida também por índice único `IX_CheckInSessions_StartedAtDateUtc` (coluna computada persistida `CAST([StartedAt] AS DATE)`).

## Serviços

- `CheckInService` — iniciar sessão, registar check-in, listar check-ins da sessão.

## Controllers

- `CheckInsController` — `/api/checkins`, `/api/checkins/sessions`, etc.

## Notas

- `TeamGeneration` depende da ordem de `CheckedInAt` neste módulo.
