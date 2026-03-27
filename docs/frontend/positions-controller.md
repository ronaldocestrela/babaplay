# PositionsController

**Base path:** `/api/positions`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/positions

Lista posições ordenadas alfabeticamente por `name`.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
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
  "name": "string"
}
```

### Resposta 200

Posição criada (mesmo formato de um item do GET).

### Resposta 400

Nome obrigatório.

---

## PUT /api/positions/{id}

Atualiza o nome de uma posição existente.

### Payload

```json
{
  "name": "string"
}
```

### Resposta 200

Posição atualizada (mesmo formato de um item do GET).

### Resposta 400

Nome obrigatório.

### Resposta 404

Posição não encontrada.

---

## DELETE /api/positions/{id}

Remove uma posição.

### Resposta 200

Eliminação bem-sucedida (envelope vazio `data: null`).

### Resposta 404

Posição não encontrada.

### Resposta 409

A posição está atribuída a um ou mais associados; remova primeiro as atribuições.
