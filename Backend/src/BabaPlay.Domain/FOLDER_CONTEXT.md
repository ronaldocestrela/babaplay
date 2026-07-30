# 📁 FOLDER_CONTEXT — BabaPlay.Domain

## Instrução para LLM

Você está no projeto **BabaPlay.Domain**. Este é o **núcleo do sistema** — a camada mais interna da Clean Architecture.

## Responsabilidade deste projeto

Contém tudo que representa **o que é** o negócio, independente de qualquer tecnologia ou framework:

- **Entities:** Classes de domínio ricas com identidade e regras de negócio encapsuladas
- **Enums:** Tipos enumerados do domínio (ex: status de partida, tipos de evento, etc.)
- **Events:** Domain Events disparados pelas entidades (ex: `PlayerCheckedIn`, `MatchFinished`)
- **Exceptions:** Exceções de domínio tipadas (ex: `DomainException`, `NotFoundException`)
- **Interfaces:** Contratos abstratos de repositórios e serviços (sem implementação)
- **ValueObjects:** Objetos de valor imutáveis sem identidade própria (ex: coordenadas GPS, dinheiro)

## Entidades principais do sistema

| Entidade              | Descrição                                                              |
|-----------------------|------------------------------------------------------------------------|
| `Player`              | Jogador cadastrado em um tenant, com posições, pontuação e status      |
| `Team`                | Time de um tenant, com seus jogadores associados                       |
| `GameDay`             | Dia de jogo programado por um tenant                                   |
| `Match`               | Partida dentro de um GameDay, com times e placar                       |
| `MatchEvent`          | Evento ocorrido durante uma partida (gol, cartão, assistência, etc.)  |
| `MatchSummary`        | Súmula/resumo de uma partida (geração de PDF)                          |
| `Checkin`             | Check-in de um jogador em um GameDay (com geolocalização)             |
| `PlayerScore`         | Pontuação acumulada de um jogador no ranking                           |
| `PlayerMonthlyFee`    | Mensalidade associada a um jogador                                     |
| `MonthlyFeePayment`   | Pagamento de uma mensalidade                                           |
| `CashTransaction`     | Transação de caixa do tenant (receita ou despesa)                      |
| `Fundraiser`          | Vaquinha/arrecadação financeira                                        |
| `Notification`        | Notificação para usuário                                               |
| `Role`                | Papel/perfil de acesso do usuário no tenant                            |
| `Permission`          | Permissão individual dentro de um role                                 |
| `Position`            | Posição de futebol (goleiro, zagueiro, etc.)                           |

## Regras para LLM ao atuar neste projeto

1. **Nenhuma dependência externa** — este projeto não pode referenciar NuGet packages de infraestrutura (EF Core, SignalR, HTTP, etc.).
2. **Lógica de negócio aqui** — validações, invariantes e regras do domínio pertencem às Entities, não aos Services da Application.
3. **Interfaces aqui, implementações fora** — `IPlayerRepository`, `ITokenService`, etc. ficam aqui; suas implementações ficam em `Infrastructure`.
4. **Entities herdam de `EntityBase`** — que contém `Id` (Guid), `CreatedAt` e `UpdatedAt` para rastreamento.
5. **Nunca** acessar banco de dados, HTTP ou qualquer I/O nesta camada.
