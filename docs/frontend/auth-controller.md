# AuthController

**Base path:** `/api/auth`  
**Auth:** não (`[AllowAnonymous]`).  
**Tenant:** **sim** — para `register` / `login` o utilizador é criado na BD do tenant; enviar `X-Tenant-Subdomain` (ou subdomínio no host).

Envelope: [api-conventions.md](api-conventions.md).

---

## POST /api/auth/register

### Payload

```json
{
  "email": "user@example.com",
  "password": "string",
  "userType": 2
}
```

- `userType` (enum, opcional; default **2** = Associate):
  - `0` — PlatformAdmin (mapeia role **Admin** no seed)
  - `1` — AssociationStaff → role **Manager**
  - `2` — Associate → role **Associate**

### Resposta 200

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOi...",
    "userId": "string",
    "roles": ["Associate"],
    "permissions": ["associates.read", "checkins.manage"]
  },
  "error": null,
  "errors": null
}
```

### Resposta 400

Erros de validação ou Identity (`errors` com mensagens).

---

## POST /api/auth/login

### Payload

```json
{
  "email": "user@example.com",
  "password": "string"
}
```

### Resposta 200

Igual à estrutura de `data` em register (`accessToken`, `userId`, `roles`, `permissions`).

### Resposta 401

Credenciais inválidas (`success: false`).

### Resposta 400

Email em falta / inválido.
