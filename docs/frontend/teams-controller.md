# TeamsController

**Base path:** `/api/teams`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

A geração usa check-ins da **sessão**: ordem de chegada (primeira hora de check-in por associado) e distribuição **round-robin** em `teamCount` equipas. Apaga equipas/membros anteriores **da mesma sessão** antes de gerar de novo.

---

## POST /api/teams/generate

### Payload

```json
{
  "sessionId": "string",
  "teamCount": 2
}
```

`teamCount` opcional; default no servidor **2**. Mínimo **2** equipas.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "sessionId": "string",
      "name": "Team 1",
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

*(Lista de equipas criadas; não inclui membros neste endpoint — usar GET by-session.)*

### Resposta 400

`teamCount < 2` ou sessão sem check-ins.

---

## GET /api/teams/by-session/{sessionId}

Lista equipas da sessão com **membros** incluídos.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "sessionId": "string",
      "name": "Team 1",
      "members": [
        {
          "id": "string",
          "teamId": "string",
          "associateId": "string",
          "order": 0,
          "createdAt": "2026-01-01T12:00:00Z",
          "updatedAt": null
        }
      ],
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

Ordenação: equipas por `name`. `order` reflete ordem na distribuição.
