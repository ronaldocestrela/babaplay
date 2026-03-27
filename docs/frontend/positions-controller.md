# PositionsController

**Base path:** `/api/positions`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/positions

Lista posições ordenadas por `sortOrder`, depois `name`.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "sortOrder": 1,
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

---

## POST /api/positions

### Payload

```json
{
  "name": "string",
  "sortOrder": 0
}
```

### Resposta 200

Posição criada (mesmo formato de um item do GET).

### Resposta 400

Nome obrigatório.
