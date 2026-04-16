# MatchReportsController

**Base path:** `/api/matchreports`
**Auth:** Bearer JWT.
**Tenant:** sim.

Envelope: [api-conventions.md](api-conventions.md).

Uma súmula pertence a uma **sessão** (`sessionId`) e pode conter **várias partidas**. Cada partida guarda estatísticas por jogador: gols, assistências, cartões amarelos, cartões vermelhos e observações.

Regras principais:
- `sessionId` deve existir no tenant.
- jogadores referenciados devem existir como associados do tenant.
- estatísticas numéricas não podem ser negativas.
- súmula finalizada só pode ser editada por utilizadores com role `Admin`.

---

## GET /api/matchreports/sessions/{sessionId}

Consulta a súmula completa de uma sessão.

### Resposta 200

```json
{
  "success": true,
  "data": {
    "id": "string",
    "sessionId": "string",
    "notes": "Observações gerais",
    "status": 0,
    "finalizedAt": null,
    "finalizedByUserId": null,
    "games": [
      {
        "id": "string",
        "gameNumber": 1,
        "title": "Jogo 1",
        "notes": "Primeira partida",
        "playerStats": [
          {
            "id": "string",
            "associateId": "string",
            "goals": 2,
            "assists": 1,
            "yellowCards": 1,
            "redCards": 0,
            "observations": "Capitão",
            "createdAt": "2026-01-01T12:00:00Z",
            "updatedAt": null
          }
        ],
        "createdAt": "2026-01-01T12:00:00Z",
        "updatedAt": null
      }
    ],
    "createdAt": "2026-01-01T12:00:00Z",
    "updatedAt": "2026-01-01T13:00:00Z"
  },
  "error": null,
  "errors": null
}
```

### Resposta 404

Sessão ainda sem súmula cadastrada.

---

## PUT /api/matchreports/sessions/{sessionId}

Cria ou atualiza a súmula completa de uma sessão.

### Payload

```json
{
  "notes": "Rodada de quarta-feira",
  "games": [
    {
      "title": "Jogo 1",
      "notes": "Time azul x time branco",
      "playerStats": [
        {
          "associateId": "string",
          "goals": 2,
          "assists": 1,
          "yellowCards": 1,
          "redCards": 0,
          "observations": "Melhor em campo"
        }
      ]
    }
  ]
}
```

### Resposta 200

Mesmo formato do `GET`, com a súmula persistida.

### Resposta 400

Validações possíveis:
- nenhuma partida informada
- `title` vazio
- estatísticas negativas
- jogador duplicado na mesma partida
- associado inexistente no tenant

### Resposta 404

Sessão inexistente.

### Resposta 403

Tentativa de editar uma súmula finalizada sem role `Admin`.

---

## POST /api/matchreports/sessions/{sessionId}/finalize

Marca a súmula da sessão como finalizada.

### Resposta 200

Mesmo formato do `GET`, com `status = 1`, `finalizedAt` e `finalizedByUserId` preenchidos.

### Resposta 401

Utilizador autenticado não resolvido no contexto da request.

### Resposta 404

Súmula não encontrada para a sessão.

---

## Notas de consumo no frontend

- `status`: `0 = Draft`, `1 = Finalized`.
- Quando o `GET` devolver `404`, o frontend pode assumir que a sessão ainda não possui súmula e abrir a tela em modo de criação.
- A edição administrativa de uma súmula finalizada mantém o estado finalizado.