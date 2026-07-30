# 📁 FOLDER_CONTEXT — BabaPlay.Infrastructure

## Instrução para LLM

Você está no projeto **BabaPlay.Infrastructure**. Esta é a camada de **implementações concretas** — onde as abstrações do domínio e da application ganham vida com tecnologias reais.

## Responsabilidade deste projeto

Implementa todas as interfaces definidas em `Application/Interfaces/` e `Domain/Interfaces/`, conectando o sistema ao mundo externo:

| Diretório         | Responsabilidade                                                                            |
|-------------------|---------------------------------------------------------------------------------------------|
| `Persistence/`    | DbContext EF Core (`AppDbContext`), configuração de entidades, Migrations do banco PostgreSQL |
| `Repositories/`   | Implementações concretas de todos os repositórios (ex: `PlayerRepository`, `MatchRepository`) |
| `Services/`       | Implementações de serviços externos (JWT, email, upload Cloudinary, PDF, SignalR notifiers, etc.) |
| `Hubs/`           | Hubs SignalR para comunicação em tempo real (check-in, eventos de partida)                  |
| `Authorization/`  | Handlers de autorização customizados baseados em roles e permissões do tenant               |
| `Entities/`       | Entidades de infraestrutura que não pertencem ao domínio (ex: RefreshToken, UserDeviceToken)|
| `Workers/`        | Background services/workers para processamento assíncrono (ex: fila de e-mails)            |
| `Settings/`       | Classes de configuração tipadas (leitura de `appsettings.json`)                             |

## Serviços implementados

| Serviço                              | Descrição                                                        |
|--------------------------------------|------------------------------------------------------------------|
| `JwtTokenService`                    | Geração e validação de tokens JWT para autenticação              |
| `ResendEmailService`                 | Envio de e-mails transacionais via Resend API                    |
| `CloudinaryImageUploader`            | Upload de imagens para Cloudinary (fotos de perfil, logos)       |
| `CloudinaryTenantLogoStorageService` | Armazenamento de logos de tenant no Cloudinary                   |
| `LocalMatchSummaryStorageService`    | Armazenamento local de PDFs de súmula de partidas                |
| `MinimalPdfMatchSummaryGenerator`    | Geração de PDFs de súmula usando QuestPDF                        |
| `SignalRCheckinRealtimeNotifier`     | Notificações em tempo real de check-in via SignalR               |
| `SignalRMatchEventRealtimeNotifier`  | Notificações em tempo real de eventos de partida via SignalR     |
| `TenantOwnerProvisioningService`     | Provisionamento de novo tenant com owner e configurações iniciais |
| `TenantOwnerRbacBootstrapService`    | Bootstrap de roles e permissões padrão para novos tenants        |
| `PasswordResetService`               | Geração e validação de tokens de reset de senha                  |
| `RequestTenantContext`               | Extrai o TenantId do contexto HTTP atual (multi-tenancy)         |

## Banco de Dados

- **SGBD:** PostgreSQL
- **ORM:** Entity Framework Core (code-first)
- **DbContext:** `AppDbContext` em `Persistence/AppDbContext.cs`
- **Migrations:** em `Persistence/Migrations/` — gerenciadas via EF Core CLI

## Regras para LLM ao atuar neste projeto

1. **Nunca expor** classes de Infrastructure diretamente à Application — sempre via interfaces.
2. **Migrations** são geradas automaticamente pelo EF Core; nunca editar arquivos de migration manualmente.
3. **Configuração de DI** está centralizada em `ServiceRegistration.cs` — ao criar um novo serviço ou repositório, registrá-lo aqui.
4. O `AppDbContext` contém o mapeamento de todas as entidades — ao adicionar uma nova Entity do Domain, criar o `DbSet` e configuração correspondente aqui.
5. **Multi-tenancy** é implementado via `RequestTenantContext` que extrai o `TenantId` do JWT/header — todos os repositórios devem filtrar por `TenantId` quando aplicável.
6. Workers em `Workers/` são `IHostedService` registrados no container DI para processamento em background.
