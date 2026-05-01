# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

### Added — Fase 1: Identity + Auth

- `ApplicationUser : IdentityUser` com propriedades `IsActive` e `CreatedAt`
- `RefreshToken` entity com rotação e revogação (`RevokedAt` nullable)
- Master DB SaaS completo: `Tenant`, `UserTenant`, `Plan`, `Subscription` (enum `SubscriptionStatus`)
- `MasterDbContext : IdentityDbContext<ApplicationUser>` com todas as tabelas e índices
- EF Core migration: `InitialMaster`
- `JwtSettings` com suporte a `SecretKey`, `Issuer`, `Audience`, `AccessTokenExpiresInMinutes`, `RefreshTokenExpiresInDays`
- `JwtTokenService` (HmacSha256, claims: sub/email/jti/iat/role, ClockSkew=Zero)
- `UserRepository` e `RefreshTokenRepository` abstraídos via interfaces de Application
- `LoginCommand` / `LoginCommandHandler` — valida credenciais, gera e persiste token par
- `RefreshTokenCommand` / `RefreshTokenCommandHandler` — valida, revoga e rotaciona refresh token
- `POST /api/v1/auth/login` → 200 `AuthResponse` | 401 INVALID_CREDENTIALS | 422 USER_INACTIVE
- `POST /api/v1/auth/refresh-token` → 200 `AuthResponse` | 401 INVALID_TOKEN/TOKEN_EXPIRED
- Swagger Bearer JWT security definition + requirement globais
- `appsettings.json` / `appsettings.Development.json` com seções `ConnectionStrings:MasterDb` e `Jwt`
- Pacotes: `Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.0`, `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`, `System.IdentityModel.Tokens.Jwt 8.9.0`, `Microsoft.EntityFrameworkCore.Sqlite 10.0.0` (Tests)
- 27 testes passando (20 unit + 7 integration)


### Added — Fase 0: Fundação

- Solução `.NET 10` com 5 projetos (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`)
- Clean Architecture com dependências corretas entre camadas
- Contratos CQRS: `ICommand<T>`, `ICommandHandler<T, R>`, `IQuery<T>`, `IQueryHandler<T, R>`
- `Result<T>` e `Result` para encapsular resultados de operações sem exceção de controle de fluxo
- Primitivos de domínio: `EntityBase`, `IAggregateRoot`, `IRepository<T>`, `IUnitOfWork`, `IDomainEvent`
- Exceções de domínio: `DomainException`, `NotFoundException`, `ValidationException`
- Vertical slice Ping: `PingCommand` + `PingCommandHandler`, `PingQuery` + `PingQueryHandler`, `PingController`
- `GlobalExceptionHandler` com mapeamento de exceções de domínio para `ProblemDetails`
- Swagger/OpenAPI configurado com docs XML
- Testes unitários e de integração para o vertical slice Ping (ciclo TDD Red-Green-Refactor)
- `coverlet` configurado para cobertura de código
- `Directory.Build.props` com configurações compartilhadas de projeto
- GitHub Actions CI: build + testes + cobertura >= 80%
