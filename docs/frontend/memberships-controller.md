# MembershipsController

**Base path:** `/api/memberships`  
**Auth:** Bearer JWT.  
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

`MembershipStatus`: `0` Pending, `1` Paid, `2` Overdue.

---

## GET /api/memberships/associate/{associateId}

Lista mensalidades do associado (mais recentes primeiro).

### Resposta 200

```json
{
  "success": true,
  "data": [
    {
      "id": "string",
      "associateId": "string",
      "year": 2026,
      "month": 3,
      "amount": 50.0,
      "status": 0,
      "payments": [],
      "createdAt": "2026-01-01T12:00:00Z",
      "updatedAt": null
    }
  ],
  "error": null,
  "errors": null
}
```

*(Em listagens, `payments` pode vir vazio ou povoado consoante include EF.)*

---

## POST /api/memberships

### Payload

```json
{
  "associateId": "string",
  "year": 2026,
  "month": 3,
  "amount": 50.0
}
```

### Resposta 200

Mensalidade criada (formato como item do GET).

### Resposta 409

Já existe mensalidade para o par associado + ano + mês.

### Resposta 400

Período inválido.

---

## POST /api/memberships/{membershipId}/payments

Regista pagamento; atualiza a mensalidade para **Paid** (`status: 1`).

Efeito adicional no caixa:
- O pagamento gera automaticamente um movimento de caixa de receita.
- Categoria usada: `Pagamento de mensalidade` (criada automaticamente se não existir).
- Esse movimento impacta o saldo acumulado de `/api/cashentries`.

### Payload

```json
{
  "amount": 50.0,
  "method": "cash"
}
```

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "membershipId": "string",
    "paidAt": "2026-01-01T12:00:00Z",
    "amount": 50.0,
    "method": "cash",
    "createdAt": "2026-01-01T12:00:00Z",
    "updatedAt": null
  },
  "error": null,
  "errors": null
}
```

### Resposta 404

Mensalidade não encontrada.
