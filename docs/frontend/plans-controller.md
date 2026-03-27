# PlansController — Plataforma

**Base path:** `GET|POST|PUT|DELETE /api/platform/plans`  
**Auth:** não (MVP: `[AllowAnonymous]`).  
**Tenant:** não — rotas plataforma.

Envelope: ver [api-conventions.md](api-conventions.md).

---

## GET /api/platform/plans

Lista todos os planos.

### Request

Sem corpo. Sem query obrigatória.

### Resposta 200 (sucesso)

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "description": "string | null",
      "monthlyPrice": 0.0,
      "maxAssociates": 100,
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": "2026-01-02T12:00:00Z | null"
    }
  ],
  "error": null,
  "errors": null
}
```

---

## GET /api/platform/plans/{id}

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "name": "string",
    "description": "string | null",
    "monthlyPrice": 0.0,
    "maxAssociates": 100,
    "createdAt": "2026-01-01T12:00:00Z",
    "updatedAt": null
  },
  "error": null,
  "errors": null
}
```

### Resposta 404

`success: false` — plano não encontrado.

---

## POST /api/platform/plans

### Payload (JSON)

```json
{
  "name": "string",
  "description": "string | null",
  "monthlyPrice": 0.0,
  "maxAssociates": 100
}
```

`maxAssociates` é opcional (`null` permitido).

### Resposta 200

Objeto plano criado (mesma forma que GET por id).

### Resposta 400

Validação (ex.: nome obrigatório).

---

## PUT /api/platform/plans/{id}

### Payload

Igual ao POST.

### Resposta 200

Plano atualizado.

### Resposta 404

Plano não encontrado.

---

## DELETE /api/platform/plans/{id}

### Resposta 200

```json
{
  "success": true,
  "data": null,
  "error": null,
  "errors": null
}
```

### Resposta 404

Plano não encontrado.
