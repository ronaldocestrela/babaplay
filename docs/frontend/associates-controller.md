# AssociatesController

**Base path:** `/api/associates`  
**Auth:** Bearer JWT (exceto `GET .../invitations/{token}`, ver abaixo).  
**Tenant:** sim — enviar `X-Tenant-Subdomain` (ou subdomínio no host) em **todas** as rotas, incluindo a validação anónima do convite.

Envelope: [api-conventions.md](api-conventions.md).

Regra: cada associado deve ter **entre 1 e 3** `positionIds` distintos.

---

## GET /api/associates

Lista associados ordenados por `name`, com posições projetadas (DTO `AssociateResponse`; sem entidades EF aninhadas).

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
      "isActive": true,
      "positions": [
        {
          "positionId": "string",
          "positionName": "Goleiro"
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

---

## GET /api/associates/{id}

### Resposta 200

Um associado com `positions` (mesma estrutura).

### Resposta 404

---

## POST /api/associates

Cria o associado e **provisiona automaticamente** um utilizador Identity com role **Associate** (ligado via `userId` / `AssociateId`). O e-mail é **obrigatório** (único no tenant); a senha inicial é gerada internamente — o associado deve usar o fluxo de redefinição de palavra-passe quando existir envio de e-mail.

### Payload

```json
{
  "name": "string",
  "email": "string",
  "phone": "string | null",
  "positionIds": ["id1", "id2"]
}
```

`positionIds`: mínimo 1, máximo 3, ids de `Position` existentes.

### Resposta 200

Associado criado (com posições e `userId` preenchido).

### Resposta 400

Validação de posições, nome ou e-mail em falta; ou falha ao criar o utilizador (ex.: e-mail duplicado).

---

## PUT /api/associates/{id}

### Payload

Igual ao POST.

### Resposta 200

Associado atualizado.

### Resposta 404 / 400

Não encontrado ou validação de posições.

---

## PATCH /api/associates/{id}/active

Ativa ou desativa o associado (`isActive`). Associados **inativos** não podem fazer login (ver [auth-controller.md](auth-controller.md)).

### Payload

```json
{
  "isActive": false
}
```

### Resposta 200

Associado atualizado (mesma estrutura que GET).

### Resposta 404

Associado não encontrado.

---

## POST /api/associates/invitations

Cria um convite para registo de novo associado (ligação com token e expiração). Exige utilizador autenticado com role **Admin** ou **Manager**. O pedido **deve** permitir resolver o tenant (`X-Tenant-Subdomain` ou subdomínio no host); caso contrário a API devolve **400** — o link gerado inclui o subdomínio para o frontend saber qual tenant usar nas chamadas seguintes.

### Payload

```json
{
  "email": "user@example.com",
  "isSingleUse": false
}
```

- `email` — opcional. Convites **single-use** ligados a e-mail podem exigir `email` consoante a regra do serviço; convites **partilhados** (multi-uso) costumam omitir `email`.
- `isSingleUse` — opcional; default **false** (link reutilizável).

### Resposta 200

```json
{
  "success": true,
  "data": {
    "token": "string",
    "email": "string | null",
    "isSingleUse": false,
    "expiresAt": "2026-01-01T12:00:00Z",
    "link": "https://frontend.example.com/convite/{token}?tenant={subdomain}"
  },
  "error": null,
  "errors": null
}
```

- `link` — URL absoluta para o frontend: base configurada em `InvitationLinkOptions:FrontendBaseUrl` (appsettings), ou em último caso `scheme://host` do pedido; path `/convite/{token}` (token URL-encoded) e query **`tenant`** com o subdomínio do tenant (URL-encoded). O cliente deve ler o parâmetro `tenant` e enviar o mesmo valor no header **`X-Tenant-Subdomain`** ao chamar `GET .../invitations/{token}` e `POST .../register-with-invitation` (ver [auth-controller.md](auth-controller.md)).

### Resposta 401 / 400

Utilizador não autenticado; falha de validação/negócio ao criar o convite; ou **tenant não resolvido** (mensagem indicando que falta `X-Tenant-Subdomain` ou host com subdomínio).

---

## GET /api/associates/invitations/{token}

Valida um convite pelo token (antes do registo). **`[AllowAnonymous]`** — não envia JWT; o **tenant** continua a ser resolvido pelo header/subdomínio. Se o utilizador chegou pela `link` devolvida na criação, use o query param **`tenant`** como valor de **`X-Tenant-Subdomain`**.

### Resposta 200

```json
{
  "success": true,
  "data": {
    "token": "string",
    "email": "string | null",
    "isSingleUse": false,
    "expiresAt": "2026-01-01T12:00:00Z"
  },
  "error": null,
  "errors": null
}
```

*(Sem campo `link` — reutilize o `link` da criação ou construa a rota com o mesmo `token` e o parâmetro `tenant` da query string.)*

### Resposta 400 / 404

Token inválido, expirado ou convite já consumido (mensagens no envelope de erro).
