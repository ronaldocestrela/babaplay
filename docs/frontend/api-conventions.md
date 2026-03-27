# Convenções da API (frontend)

## Base URL

Definir por ambiente (ex.: `https://api.exemplo.com`). Em desenvolvimento costuma incluir porta da `dotnet run`.

## Multitenancy (rotas **não** plataforma)

Todas as rotas exceto `/api/platform/*`, `/swagger`, `/health` precisam de contexto de tenant:

1. **Subdomínio** no host (ex.: `https://clubexemplo.localhost:7xxx`), ou  
2. Header **`X-Tenant-Subdomain`**: valor igual ao `subdomain` do tenant (ex.: `clubexemplo`).

Sem tenant válido, a API pode responder **400** com corpo JSON `{ "error": "..." }` (erro direto do middleware, **sem** o envelope `success`/`data` neste caso).

## Envelope de resposta (`ApiResponse<T>`)

Todas as respostas geradas pelos controllers via `FromResult` seguem o padrão:

### Sucesso (HTTP 200, 201 não usado no MVP para POST)

```json
{
  "success": true,
  "data": <T ou null>,
  "error": null,
  "errors": null
}
```

- Em operações sem corpo (ex.: `DELETE` com sucesso), `data` costuma ser **`null`**.

### Erro (HTTP 400, 401, 403, 404, 409 conforme caso)

```json
{
  "success": false,
  "data": null,
  "error": "mensagem principal",
  "errors": ["mensagem1", "mensagem2"]
}
```

- `errors` pode ser lista vazia; use `error` e/ou `errors` para exibir validação.

## Serialização JSON

ASP.NET Core usa **camelCase** por omissão (`PropertyNamingPolicy.CamelCase`). Exemplos: `createdAt`, `monthlyPrice`, `associateId`.

## Autenticação (rotas `[Authorize]`)

- Header: **`Authorization: Bearer <accessToken>`**  
- O token é devolvido em `POST /api/auth/login` e `POST /api/auth/register` dentro de `data.accessToken`.

## CORS

Origens permitidas incluem entradas na tabela **AllowedOrigins** (BD plataforma) e **localhost** na política dinâmica. Ajustar conforme ambiente.
