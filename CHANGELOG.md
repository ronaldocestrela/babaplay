# Changelog

Todas as mudanças relevantes deste projeto serão documentadas aqui.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/).

---

## [Unreleased]

### Added — Fase 0: Fundação

- Solução `.NET 8` com 5 projetos (`Api`, `Application`, `Domain`, `Infrastructure`, `Tests`)
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
