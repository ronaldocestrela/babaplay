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
  "name": "string",
  "email": "user@example.com",
  "password": "string",
  "userType": 2
}
```

- `name` (obrigatório): nome usado para criar o registo de `Associate` quando aplicável.

- `userType` (enum, opcional; default **2** = Associate):
  - `0` — PlatformAdmin (mapeia role **Admin** no seed)
  - `1` — AssociationStaff → role **Manager**
  - `2` — Associate → role **Associate**

### Regra de negócio no signup

- `AssociationStaff` e `Associate` criam automaticamente um registo de `Associate` no tenant e ligam `associateId` no utilizador Identity.
- `PlatformAdmin` não cria `Associate`.
- Se houver falha parcial no fluxo (ex.: cria utilizador mas falha ao criar associado), o backend faz rollback compensatório e devolve erro.

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

Erros de validação, Identity ou sincronização com Associate (`errors` com mensagens).

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

### Resposta 403

Conta de associado **inativa** (`Associate.isActive === false` na BD do tenant). O utilizador existe e a password está correta, mas o login é recusado. Gerir estado com `PATCH /api/associates/{id}/active`.

### Resposta 400

Email em falta / inválido.
