# PermissionsController

**Base path:** `/api/permissions`  
**Auth:** **Bearer JWT** obrigatório.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/permissions

Lista permissões registadas na BD do tenant.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "associates.read",
      "description": "string | null",
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

Ordenação: por `name`. Os nomes são usados nas policies JWT `perm:<nome>` e nas claims `permission`.
