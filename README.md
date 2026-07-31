# BabaPlay

Plataforma SaaS multitenant para gestão de associações esportivas, ligas e peladas de futebol.

Cada **tenant** é uma associação com jogadores, partidas, financeiro, comunicação e configurações isoladas. Versão atual: **v0.2.0-beta.1**.

## Funcionalidades

| Módulo | O que cobre |
|--------|-------------|
| **Identity** | Onboarding de associações, membros, JWT + refresh, RBAC por tenant |
| **Sports** | Game days, check-in/RSVP, sorteio de times, súmula, MVP, rankings |
| **Financial** | Mensalidades, faturas, PIX, inadimplentes, extrato, vaquinhas |
| **Communication** | Mural de avisos, enquetes, alertas, chat em tempo real (SignalR) |
| **Settings** | Dados do tenant, logo, agenda semanal, link de convite |

## Stack

- **.NET 10** — ASP.NET Core Web API + Blazor WebAssembly
- **EF Core** + **SQL Server**
- **ASP.NET Core Identity** + JWT
- **SignalR** (chat)
- **Docker / Compose** + Nginx (deploy)
- **xUnit / bUnit** (testes; CI exige cobertura ≥ 80%)

Frontend ativo: `Backend/src/BabaPlay.Web/`. A pasta `web/` (React/Vite) é legado e não deve receber features novas.

## Arquitetura

Monólito modular com Clean Architecture e CQRS:

```
Backend/
├── BabaPlay.Api            # Controllers, hubs, Program.cs
├── BabaPlay.Application    # Commands, Queries, handlers, RBAC
├── BabaPlay.Domain         # Entidades e regras de domínio
├── BabaPlay.Infrastructure # EF Core, Identity, e-mail, storage
├── BabaPlay.Web            # Blazor WASM (UI)
└── BabaPlay.Tests          # Testes unitários e de integração
```

- Controllers só recebem HTTP e despacham para handlers.
- Persistência via Repository + Unit of Work (sem `DbContext` nos handlers).
- Isolamento por tenant: header `X-Tenant-Slug` + filtros/policies na API.
- Blazor fala **apenas** com a API (HTTP/SignalR); nunca acessa o banco.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local ou container)
- Docker 24+ (opcional, para deploy Compose)

## Desenvolvimento local

### 1. Banco

Ajuste a connection string em `Backend/src/BabaPlay.Api/appsettings.Development.json` (ou variáveis de ambiente):

```
ConnectionStrings__MasterDb=Server=localhost;Database=BabaPlay_Master;User Id=sa;Password=...;TrustServerCertificate=true;
```

Migrations rodam na subida da API em Development (conforme `Program.cs`).

### 2. API

```bash
cd Backend
dotnet restore BabaPlay.slnx
dotnet run --project src/BabaPlay.Api/BabaPlay.Api.csproj --launch-profile http
```

- API: `http://localhost:5050`
- Swagger: `http://localhost:5050/swagger`

### 3. Blazor WASM

```bash
cd Backend
dotnet run --project src/BabaPlay.Web/BabaPlay.Web.csproj
```

`wwwroot/appsettings.json` aponta `ApiBaseUrl` para `http://localhost:5050`. CORS da API já inclui as origens locais do Blazor.

### 4. Testes

```bash
cd Backend
dotnet test BabaPlay.slnx --configuration Release
```

Ou via script: `scripts/run-tests-local.sh`.

## Deploy com Docker

Guia completo: [`docs/deploy-manual-docker.md`](docs/deploy-manual-docker.md).

Resumo:

```bash
cp deploy/docker/.env.manual.example deploy/docker/.env.manual
# edite segredos e connection strings
docker compose -f deploy/docker/docker-compose.manual.yml --env-file deploy/docker/.env.manual up -d --build
```

Portas padrão: **8080** (web), **5050** (API), **1433** (SQL Server).

## Estrutura do repositório

| Caminho | Conteúdo |
|---------|----------|
| `Backend/` | Solução .NET (API, Web, testes) |
| `deploy/` | Docker Compose e env de exemplo |
| `docs/` | Deploy e procedimentos |
| `scripts/` | CI/testes locais |
| `web/` | Frontend React legado (migração) |
| `architecture.md` | Visão de arquitetura |
| `functions.md` | Mapa de funcionalidades |
| `blazor_functions_guide.md` | Fases da UI Blazor |
| `roadmap.md` | Planejamento de produto |
| `CHANGELOG.md` | Histórico de versões |
| `agents.md` | Convenções para agentes de código |

## Multitenancy (resumo)

1. Usuário autentica e recebe JWT.
2. Cliente envia `Authorization: Bearer …` e `X-Tenant-Slug: <slug>`.
3. A API resolve o tenant, aplica membership/RBAC e isola dados.
4. Hubs SignalR (`/hubs/*`) também exigem `X-Tenant-Slug`; no WASM o JWT vai em `?access_token=` no WebSocket.

## Documentação

- [Arquitetura](architecture.md)
- [Funcionalidades](functions.md)
- [Guia Blazor](blazor_functions_guide.md)
- [Design UI](design.md)
- [Changelog](CHANGELOG.md)
- [Deploy Docker](docs/deploy-manual-docker.md)

## Licença

Uso privado / projeto em desenvolvimento. Contate os mantenedores para termos de distribuição.
