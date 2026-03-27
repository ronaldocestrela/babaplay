# CashEntriesController

**Base path:** `/api/CashEntries` (também aceite como `/api/cashentries` — routing **case-insensitive** por omissão.)

**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/CashEntries

Lista movimentos de caixa com categoria incluída, mais recentes primeiro.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "amount": 25.5,
      "categoryId": "string",
      "description": "string | null",
      "entryDate": "2026-01-01T12:00:00Z",
      "category": {
        "id": "string",
        "name": "string",
        "createdAt": "2026-01-01T12:00:00Z",
        "updatedAt": null
      },
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

---

## POST /api/CashEntries

### Payload

```json
{
  "amount": 25.5,
  "categoryId": "string",
  "description": "string | null",
  "entryDate": "2026-01-01T12:00:00Z"
}
```

`entryDate` opcional; se omitido, o servidor usa data/hora UTC atual.

### Resposta 200

Movimento criado (estrutura como um item do GET; `category` pode ou não vir expandido conforme include após save).

### Resposta 404

`categoryId` não existe.
