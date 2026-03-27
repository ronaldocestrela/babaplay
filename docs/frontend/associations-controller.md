# AssociationsController

**Base path:** `/api/associations`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/associations

Lista associações no tenant.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "address": "string | null",
      "regulation": "string | null",
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

---

## GET /api/associations/{id}

### Resposta 200

Um objeto no formato do item acima.

### Resposta 404

---

## POST /api/associations

Upsert: se `id` for `null` ou omitido no fluxo atual — o body inclui `id` opcional.

### Payload

```json
{
  "id": "string | null",
  "name": "string",
  "address": "string | null",
  "regulation": "string | null"
}
```

- `id` **null** ou ausente: cria novo registo.  
- `id` preenchido: atualiza o existente.

### Resposta 200

Associação criada ou atualizada.

### Resposta 400 / 404

Validação ou associação não encontrada (update).
