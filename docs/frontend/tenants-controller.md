# TenantsController — Plataforma

**Base path:** `/api/platform/tenants`  
**Auth:** não (MVP: `[AllowAnonymous]`).  
**Tenant:** não.

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/platform/tenants

Lista tenants.

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "name": "string",
      "subdomain": "string",
      "databaseName": "string",
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

---

## GET /api/platform/tenants/{id}

### Resposta 200

Objeto tenant (mesma forma que um item do array acima).

### Resposta 404

Tenant não encontrado.

---

## POST /api/platform/tenants

### Payload

```json
{
  "name": "string",
  "subdomain": "string"
}
```

- `subdomain` normalizado para minúsculas no servidor.

### Resposta 200

Tenant criado (inclui `databaseName` gerado).

### Resposta 400 / 409

Validação ou subdomínio em uso (`409 Conflict`).

---

## PUT /api/platform/tenants/{id}

### Payload

Igual ao POST.

### Resposta 200

Tenant atualizado.

### Resposta 404 / 409

Não encontrado ou subdomínio duplicado.

---

## DELETE /api/platform/tenants/{id}

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

---

## POST /api/platform/tenants/{id}/subscription

Cria subscrição e **provisiona** a base de dados do tenant (migrações + seed técnico).

### Payload

```json
{
  "planId": "string"
}
```

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "tenantId": "string",
    "planId": "string",
    "startDate": "2026-01-01T12:00:00Z",
    "endDate": null,
    "status": 0,
    "createdAt": "2026-01-01T12:00:00Z",
    "updatedAt": null
  },
  "error": null,
  "errors": null
}
```

`status`: `0` Active, `1` Suspended, `2` Cancelled (`SubscriptionStatus`).

### Resposta 404

Tenant ou plano não encontrado.

### Resposta 400

Falha no provisionamento (mensagem em `error` / `errors`).
