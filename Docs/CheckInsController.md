# Check-ins (Presença diária)

Este documento descreve os endpoints criados em `WebApi/Controllers/CheckInsController.cs` para que o front-end (Blazor) possa registrar check-ins e montar times a partir da ordem de chegada.

> **Nota**: todos os endpoints exigem **autenticação** (Bearer token). O back-end usa o usuário logado para vincular o check-in ao `Associado` correspondente.

---

## Base da API

- **Prefixo base:** `/api/checkins`
- **Autorização:** `Authorization: Bearer <token>`

---

## 1) Registrar check-in do dia

### Endpoint

| Método | URL | Body |
|--------|-----|------|
| POST | `/api/checkins` | *sem corpo* |

### Comportamento
- Registra o check-in do usuário logado para o dia atual (UTC).
- Só permite **um check-in por associado por dia**.

### Resposta de sucesso
O serviço retorna um `ResponseWrapper<T>` que contém um objeto similar a:

```json
{
  "isSuccessful": true,
  "data": {
    "id": "...",
    "associadoId": "...",
    "fullName": "Nome do Associado",
    "positions": ["GK", "FW"],
    "arrivalOrder": 1,
    "date": "2026-03-18T00:00:00Z",
    "checkInAtUtc": "2026-03-18T14:21:05.123Z"
  },
  "message": "Check-in registrado com sucesso."
}
```

> **Observação:** o campo `positions` utiliza o enum `BabaPlayShared.Library.Enum.SoccerPosition`.

---

## 2) Listar check-ins do dia (ordem de chegada)

### Endpoint

| Método | URL | Query params |
|--------|-----|--------------|
| GET | `/api/checkins/today` | nenhum |

### Resposta de sucesso
Retorna um `ResponseWrapper<List<CheckInResponse>>` com todos os check-ins de hoje, ordenados por horário de chegada (`CheckInAtUtc`).

Exemplo de `data`:

```json
[
  {
    "id": "...",
    "associadoId": "...",
    "fullName": "Associado A",
    "positions": ["CM"],
    "arrivalOrder": 1,
    "date": "2026-03-18T00:00:00Z",
    "checkInAtUtc": "2026-03-18T14:21:05.123Z"
  },
  {
    "id": "...",
    "associadoId": "...",
    "fullName": "Associado B",
    "positions": ["ST"],
    "arrivalOrder": 2,
    "date": "2026-03-18T00:00:00Z",
    "checkInAtUtc": "2026-03-18T14:22:10.456Z"
  }
]
```

---

## 3) Montar times automaticamente (Team Assignment)

### Endpoint

| Método | URL | Query params |
|--------|-----|--------------|
| GET | `/api/checkins/teams` | `date` (opcional, formato ISO `yyyy-MM-dd`) |

### Uso
- Se `date` for omitido, o back-end usa a data atual (UTC).
- Retorna dois times (`teamA` e `teamB`) com distribuição simples e equilibrada.

### Resposta de sucesso
O `data` retornado tem a forma:

```json
{
  "date": "2026-03-18T00:00:00Z",
  "teamA": [ /* lista de CheckInResponse */ ],
  "teamB": [ /* lista de CheckInResponse */ ]
}
```

---

## Dicas para o front-end (Blazor)

- Use o `HttpClient` configurado com `Authorization` header (Bearer token).
- A resposta é um *wrapper* com o formato:
  - `isSuccessful` (bool)
  - `data` (payload específico)
  - `message` (string)

### Exemplo de uso em Blazor (pseudo):

```csharp
var response = await HttpClient.PostAsync("api/checkins", null);
var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<CheckInResponse>>();

if (wrapper?.IsSuccessful == true)
{
    var checkIn = wrapper.Data;
    // ...mostrar na UI
}
```

> Os tipos `ResponseWrapper<T>`, `CheckInResponse` e `TeamAssignmentResponse` fazem parte do backend, mas a estrutura JSON retornada deve ser compatível com o que o front espera.

---

## Campos importantes retornados (para UI)

- `arrivalOrder`: posição na fila de chegada (1 = primeiro a chegar)
- `date`: data do check-in (UTC)
- `checkInAtUtc`: timestamp exato do check-in (UTC)
- `positions`: lista de posições definidas em `BabaPlayShared.Library.Enum.SoccerPosition`

---

## Próximos passos (opcionais)
- Adicionar tela de “Check-ins do dia” com ordenação / filtro por posição
- Mostrar times gerados em cards separados (Team A / Team B)
- Permitir recarregar com data customizada via `?date=YYYY-MM-DD`
