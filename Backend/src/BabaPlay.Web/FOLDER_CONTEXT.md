# 📁 FOLDER_CONTEXT — BabaPlay.Web (Blazor)

## Instrução para LLM

Você está no projeto **BabaPlay.Web**. Este é o **novo frontend Blazor WebAssembly** do BabaPlay, construído em .NET 10. Ele está sendo desenvolvido em substituição ao frontend legado React (`/web/`).

## Responsabilidade deste projeto

Implementar toda a interface de usuário do BabaPlay como uma Single Page Application (SPA) em Blazor WebAssembly, consumindo a `BabaPlay.Api` via HTTP:

| Diretório / Arquivo  | Responsabilidade                                                                           |
|----------------------|--------------------------------------------------------------------------------------------|
| `Pages/`             | Páginas roteáveis da aplicação (cada página tem sua diretiva `@page`)                     |
| `Components/`        | Componentes Razor reutilizáveis organizados por domínio                                    |
| `Services/`          | Serviços do cliente Blazor (chamadas HTTP, estado, autenticação)                           |
| `Models/`            | ViewModels e modelos de dados do frontend (distintos dos DTOs da API)                      |
| `wwwroot/`           | Assets estáticos: CSS, JS, imagens, `index.html` (host da SPA)                            |
| `App.razor`          | Componente raiz: configura roteamento e AuthorizeRouteView                                 |
| `_Imports.razor`     | Usings globais para todos os componentes Razor                                             |
| `Program.cs`         | Ponto de entrada do Blazor WASM: configura DI, HttpClient, autenticação                   |

## Estrutura de Pages (páginas roteáveis)

| Pasta             | Páginas e responsabilidade                                          |
|-------------------|---------------------------------------------------------------------|
| `Auth/`           | Login, cadastro, onboarding do tenant, reset de senha               |
| `Dashboard/`      | Dashboard principal com KPIs e resumo do tenant                     |
| `Players/`        | Listagem, detalhe e gestão de jogadores                             |
| `Teams/`          | Listagem, criação e gestão de times                                 |
| `Matches/`        | Listagem, criação e gestão de partidas e eventos                    |
| `Financial/`      | Gestão financeira: mensalidades, caixa, vaquinhas                   |
| `Reports/`        | Relatórios e súmulas de partidas em PDF                             |
| `Settings/`       | Configurações do tenant, geolocalização, tipos de evento            |
| `Tenant/`         | Onboarding de tenant e convites de associação                       |

## Estrutura de Components (reutilizáveis)

| Pasta         | Responsabilidade                                              |
|---------------|---------------------------------------------------------------|
| `Layout/`     | Layout principal (sidebar, navbar, footer)                    |
| `Common/`     | Componentes genéricos (loading spinner, toasts, modais, etc.) |
| `Dashboard/`  | Componentes específicos do dashboard                          |
| `Players/`    | Cards, tabelas e formulários de jogadores                     |
| `Teams/`      | Componentes de times                                          |
| `Matches/`    | Componentes de partida e eventos em tempo real                |
| `Financial/`  | Componentes de gestão financeira                              |
| `Reports/`    | Componentes de relatórios                                     |
| `Settings/`   | Componentes de configuração                                   |

## Estrutura de Services

| Pasta / Arquivo               | Responsabilidade                                                              |
|-------------------------------|-------------------------------------------------------------------------------|
| `Services/Http/`              | Serviços de cliente HTTP para cada domínio da API (ex: `PlayerApiService`)   |
| `Services/State/`             | Estado global da sessão do usuário (`UserSessionState`, `TenantState`)        |
| `Services/State/CustomAuthStateProvider.cs` | Provider customizado de autenticação Blazor baseado em JWT/localStorage |
| `Services/Handlers/`          | HTTP message handlers (ex: token refresh automático)                         |
| `Services/Helpers/`           | Utilitários de serviço (ex: formatação de datas, parsing de resposta HTTP)   |

## Regras para LLM ao atuar neste projeto

1. **Consultar `blazor_functions_guide.md`** na raiz do projeto para saber quais fases/funcionalidades já foram implementadas e quais estão pendentes.
2. **Componentes Razor** usam `@inject` para injetar serviços — nunca acessar HTTP diretamente nas Pages.
3. **Autenticação** é gerenciada pelo `CustomAuthStateProvider` — use `[Authorize]` nas páginas protegidas.
4. **Estado do usuário e tenant** ficam em `UserSessionState` e `TenantState` — não duplicar estado em componentes individuais.
5. **Serviços HTTP** ficam em `Services/Http/` e implementam interfaces (`IPlayerApiService`, etc.) para facilitar testes.
6. Este é um **Blazor WebAssembly** — todo código roda no navegador; não há acesso direto ao banco de dados ou ao servidor.
7. **Seguir as convenções Razor** — nome de arquivo = nome do componente, cada componente em seu próprio arquivo `.razor`.
8. **Estado de loading e erros** deve ser tratado em cada componente/página individualmente com flags `isLoading` e `errorMessage`.
