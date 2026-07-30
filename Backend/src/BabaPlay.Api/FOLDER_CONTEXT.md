# 📁 FOLDER_CONTEXT — BabaPlay.Api

## Instrução para LLM

Você está no projeto **BabaPlay.Api**. Esta é a **camada de entrada HTTP** do sistema — a REST API que expõe os casos de uso do BabaPlay para clientes externos (frontend Blazor, aplicativos mobile, etc.).

## Responsabilidade deste projeto

Recebe requisições HTTP, autentica e autoriza o usuário, delega à camada Application e retorna respostas formatadas:

| Diretório / Arquivo          | Responsabilidade                                                                      |
|------------------------------|---------------------------------------------------------------------------------------|
| `Controllers/`               | Controllers REST organizados por domínio de negócio                                  |
| `Filters/`                   | Action filters globais (ex: validação de modelo, tratamento de exceções)             |
| `Middlewares/`               | Middlewares ASP.NET (ex: extração de TenantId, logging de requisição)                |
| `Program.cs`                 | Ponto de entrada da aplicação: configuração do host, DI, pipeline HTTP               |
| `appsettings.json`           | Configurações da aplicação (connection strings, JWT secrets, Cloudinary, etc.)       |
| `appsettings.Development.json` | Overrides de configuração para ambiente de desenvolvimento                         |
| `storage/`                   | Armazenamento local de arquivos em desenvolvimento (PDFs de súmula, logos)          |

## Controllers existentes

| Controller                    | Rota base          | Domínio                                            |
|-------------------------------|--------------------|----------------------------------------------------|
| `AuthController`              | `/api/auth`        | Login, logout, refresh token, reset de senha       |
| `PlayerController`            | `/api/players`     | CRUD de jogadores, foto, posições, mensalidades     |
| `TeamController`              | `/api/teams`       | CRUD de times e associação de jogadores             |
| `MatchController`             | `/api/matches`     | CRUD de partidas, placar, finalização               |
| `MatchEventController`        | `/api/match-events`| Registro e deleção de eventos de partida           |
| `MatchEventTypeController`    | `/api/match-event-types` | Tipos de eventos configuráveis por tenant    |
| `MatchSummaryController`      | `/api/match-summaries` | Geração e download de PDF de súmula            |
| `CheckinController`           | `/api/checkins`    | Check-in de jogadores com validação de geolocalização |
| `GameDayController`           | `/api/game-days`   | CRUD de dias de jogo                               |
| `FinancialController`         | `/api/financial`   | Mensalidades, pagamentos, caixa, vaquinhas         |
| `TenantController`            | `/api/tenants`     | Configurações do tenant, logo, geolocalização       |
| `AssociationInviteController` | `/api/invites`     | Criação e aceitação de convites de associação      |
| `RankingController`           | `/api/ranking`     | Ranking de jogadores por pontuação                  |
| `DashboardController`         | `/api/dashboard`   | Dados consolidados para o dashboard                 |
| `RoleController`              | `/api/roles`       | Gestão de roles e permissões RBAC                  |
| `PositionController`          | `/api/positions`   | CRUD de posições de futebol do tenant              |
| `PingController`              | `/api/ping`        | Health check e readiness probe                     |

## Regras para LLM ao atuar neste projeto

1. **Controllers apenas orquestram** — recebem a request, constroem o Command/Query, dispatcam para a Application e retornam o resultado. Zero lógica de negócio aqui.
2. **Autorização** via `[Authorize]` com policies baseadas em roles/permissões do tenant (RBAC implementado em `Infrastructure/Authorization/`).
3. **Todos os endpoints são multi-tenant** — o `TenantId` é extraído automaticamente pelo middleware e injetado via `ITenantContext`.
4. **Nunca injetar repositórios** diretamente nos controllers — sempre usar handlers da camada Application.
5. Ao criar um novo endpoint, seguir o padrão: `[HttpPost] public async Task<IActionResult> Method([FromBody] RequestDto dto, CancellationToken ct)`.
6. Respostas de erro são tratadas de forma centralizada pelos `Filters/` — não duplicar tratamento de exceções nos controllers.
