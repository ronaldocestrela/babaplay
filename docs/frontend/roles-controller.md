# RolesController

**Base path:** `/api/roles`  
**Auth:** **Bearer JWT** obrigatório.  
**Tenant:** sim (`X-Tenant-Subdomain` ou subdomínio).

Envelope: [api-conventions.md](api-conventions.md).

---

## GET /api/roles

Lista nomes das roles (ex.: `Admin`, `Manager`, `Associate`).

### Resposta 200

```json
{
  "success": true,
  "data": ["Admin", "Associate", "Manager"],
  "error": null,
  "errors": null
}
```

(`data` é array de strings, ordenado.)

---

## POST /api/roles/users/{userId}/assign/{roleName}

Atribui uma role a um utilizador (idempotente se já tiver a role).

### Parâmetros de rota

- `userId` — id do utilizador (`ApplicationUser.Id`)
- `roleName` — nome da role (ex.: `Manager`)

Sem corpo JSON.

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

Utilizador ou role não existe.

### Resposta 400

Falha do Identity ao adicionar role.
