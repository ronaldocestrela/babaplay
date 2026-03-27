# AssociatesController

**Base path:** `/api/associates`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

Regra: cada associado deve ter **entre 1 e 3** `positionIds` distintos.

---

## GET /api/associates

Lista associados com posições incluídas (conforme query EF).

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "email": "string | null",
      "phone": "string | null",
      "userId": "string | null",
      "positions": [
        {
          "id": "string",
          "associateId": "string",
          "positionId": "string",
          "position": {
            "id": "string",
            "name": "Goleiro",
            "sortOrder": 1,
            "createdAt": "2026-01-01T12:00:00Z",
            "updatedAt": null
          },
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

*(A forma exacta de aninhamento pode incluir `associate: null` nos links — ignorar no UI se vier.)*

---

## GET /api/associates/{id}

### Resposta 200

Um associado com `positions` (mesma estrutura).

### Resposta 404

---

## POST /api/associates

### Payload

```json
{
  "name": "string",
  "email": "string | null",
  "phone": "string | null",
  "positionIds": ["id1", "id2"]
}
```

`positionIds`: mínimo 1, máximo 3, ids de `Position` existentes.

### Resposta 200

Associado criado (com posições).

### Resposta 400

Validação de posições ou nome em falta.

---

## PUT /api/associates/{id}

### Payload

Igual ao POST.

### Resposta 200

Associado atualizado.

### Resposta 404 / 400

Não encontrado ou validação de posições.
