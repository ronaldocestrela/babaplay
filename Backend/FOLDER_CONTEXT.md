# 📁 FOLDER_CONTEXT — Backend/

## Instrução para LLM

Você está no diretório **Backend/** do projeto BabaPlay. Esta pasta contém toda a solução .NET 10 do sistema, organizada em Clean Architecture.

## Responsabilidade desta pasta

Esta pasta é a **solução completa do backend** do BabaPlay. Ela contém:

- O arquivo de solução `BabaPlay.slnx` que agrupa todos os projetos .NET
- `Dockerfile` para containerização do backend
- `Directory.Build.props` com propriedades compartilhadas entre projetos (versão do .NET, warnings, etc.)
- O diretório `src/` com todos os projetos da solução

## Projetos na pasta `src/`

| Projeto                   | Tipo              | Responsabilidade                                                            |
|---------------------------|-------------------|-----------------------------------------------------------------------------|
| `BabaPlay.Domain`         | Class Library     | Núcleo do domínio: Entities, Enums, Events, Exceptions, Interfaces, ValueObjects |
| `BabaPlay.Application`    | Class Library     | Casos de uso: Commands, Queries, DTOs, Services, Interfaces de aplicação    |
| `BabaPlay.Infrastructure` | Class Library     | Implementações concretas: Repositórios, DbContext, Serviços externos, Workers, SignalR Hubs |
| `BabaPlay.Api`            | Web API (.NET)    | Camada de entrada HTTP: Controllers REST, Middlewares, Filters, configuração do app |
| `BabaPlay.Web`            | Blazor WASM       | Frontend Blazor WebAssembly: Pages, Components, Services HTTP client, State |
| `BabaPlay.Tests`          | xUnit Test Project| Testes automatizados: Unit, Integration e Web (Blazor) tests               |

## Arquitetura Adotada

**Clean Architecture** com separação estrita de camadas:

```
Domain (núcleo, sem dependências externas)
   ↑
Application (casos de uso, depende apenas do Domain)
   ↑
Infrastructure (implementações concretas, depende de Application e Domain)
   ↑
Api / Web (camadas de entrada, dependem de Application e Infrastructure)
```

**CQRS** (Command Query Responsibility Segregation) é aplicado na camada Application:
- `Commands/` — operações de escrita (criar, atualizar, deletar)
- `Queries/` — operações de leitura (consultas, listagens)

## Regras para LLM ao atuar nesta pasta

1. **Nunca colocar** lógica de negócio diretamente em `BabaPlay.Api` ou `BabaPlay.Web` — tudo passa pela camada `Application`.
2. **Nunca referenciar** `Infrastructure` a partir de `Domain` ou `Application` — o fluxo de dependência é unidirecional (Domain ← Application ← Infrastructure ← Api/Web).
3. Ao adicionar funcionalidade nova: criar Command/Query em `Application` → implementar Repository em `Infrastructure` → expor endpoint em `Api` → criar componente em `Web`.
4. Migrations do banco de dados ficam em `Infrastructure/Persistence/Migrations/` e são gerenciadas pelo Entity Framework Core.
5. A configuração de DI (Injeção de Dependências) é centralizada nos arquivos `ServiceRegistration.cs` de cada projeto.
6. O projeto `BabaPlay.Web` é um **Blazor WebAssembly** que consome a API via HTTP — não possui acesso direto ao banco de dados.
