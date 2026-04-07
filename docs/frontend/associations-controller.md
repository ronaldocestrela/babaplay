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
      "playersPerTeam": 5,
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
  "regulation": "string | null",
  "playersPerTeam": 5
}
```

- `id` **null** ou ausente: cria novo registo.  
- `id` preenchido: atualiza o existente.
- `playersPerTeam` — opcional no JSON; se omitido, o servidor usa **5**. Usado na geração de times (`POST /api/teams/generate`). Valor mínimo **2**; pedidos com valor inferior devolvem **400**.

### Resposta 200

Associação criada ou atualizada.

### Resposta 400 / 404

Validação (nome vazio, `playersPerTeam` &lt; 2) ou associação não encontrada (update).
