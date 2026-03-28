# CategoriesController

**Base path:** `/api/categories`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/categories

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "type": 0,
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

---

## POST /api/categories

### Payload

```json
{
  "name": "string",
  "type": 0
}
```

`type` obrigatório:
- `0` = `Income` (receita)
- `1` = `Expense` (despesa)

### Resposta 200

Categoria criada.

### Resposta 400

Nome obrigatório ou tipo inválido.
