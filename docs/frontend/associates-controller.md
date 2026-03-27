# AssociatesController

**Base path:** `/api/associates`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

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
