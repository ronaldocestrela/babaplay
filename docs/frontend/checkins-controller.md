# CheckInsController

**Base path:** `/api/checkins`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

Regra: **no máximo um check-in por associado por dia** (UTC); repetido no mesmo dia → **409 Conflict**.

---

## POST /api/checkins/sessions

Abre uma nova sessão de check-in. `createdByUserId` é preenchido no servidor com o utilizador do token (`sub` / `NameIdentifier`).

### Request

Sem corpo.

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "startedAt": "2026-01-01T12:00:00Z",
    "endedAt": null,
    "createdByUserId": "string | null",
    "createdAt": "2026-01-01T12:00:00Z",
    "updatedAt": null
  },
  "error": null,
  "errors": null
}
```

---

## POST /api/checkins

Regista check-in de um associado na sessão.

### Payload

```json
{
  "sessionId": "string",
  "associateId": "string"
}
```

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "sessionId": "string",
    "associateId": "string",
    "checkedInAt": "2026-01-01T12:30:00Z",
    "createdAt": "2026-01-01T12:30:00Z",
    "updatedAt": null
  },
  "error": null,
  "errors": null
}
```

### Resposta 404

Sessão não encontrada.

### Resposta 409

Associado já fez check-in hoje (regra de negócio).

---

## GET /api/checkins/sessions/{sessionId}

Lista check-ins da sessão, ordenados por `checkedInAt`.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "sessionId": "string",
      "associateId": "string",
      "checkedInAt": "2026-01-01T12:00:00Z",
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```
