# AGENTS — BabaPlay.Modules.CheckIns

## Domínio

**CheckInSession**, **CheckIn**.

## Regras

- **Um check-in por dia e por associado** (UTC): verificação em `CheckInService.RegisterCheckInAsync`.

## Serviços

- `CheckInService` — iniciar sessão, registar check-in, listar check-ins da sessão.

## Controllers

- `CheckInsController` — `/api/checkins`, `/api/checkins/sessions`, etc.

## Notas

- `TeamGeneration` depende da ordem de `CheckedInAt` neste módulo.
