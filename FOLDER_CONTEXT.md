# 📁 FOLDER_CONTEXT — Raiz do Projeto BabaPlay

## Instrução para LLM

Você está na **raiz do projeto BabaPlay**. Este é um sistema de gestão de peladas (jogos amadores de futebol) multi-tenant, que permite a times e associações gerenciar jogadores, partidas, check-ins, financeiro, ranking e notificações.

## Responsabilidade desta pasta

Esta pasta é o **monorepo raiz** do projeto. Ela agrupa todas as partes do sistema em um único repositório versionado no Git. Aqui ficam:

- Documentação de alto nível do projeto
- Configurações compartilhadas de CI/CD e Git
- Subdiretórios com os projetos de Backend, Frontend e infraestrutura

## Estrutura Principal

| Diretório / Arquivo         | Responsabilidade                                                               |
|-----------------------------|--------------------------------------------------------------------------------|
| `Backend/`                  | Solução .NET 10 com Clean Architecture (API, Domain, Application, Infrastructure, Web Blazor, Tests) |
| `web/`                      | Frontend legado em React + TypeScript (Vite) — em processo de migração para Blazor |
| `deploy/`                   | Configurações de deploy via Docker Compose (ambientes manual/produção)         |
| `scripts/`                  | Scripts utilitários para CI local e execução de testes                         |
| `docs/`                     | Documentação complementar de deploy e procedimentos operacionais               |
| `agents.md`                 | Regras e diretrizes para agentes de IA trabalharem neste projeto               |
| `blazor_functions_guide.md` | Guia de fases de migração do frontend React para Blazor — roadmap detalhado por fase |
| `functions.md`              | Mapa de funcionalidades do sistema organizadas por domínio                     |
| `architecture.md`           | Visão geral da arquitetura do sistema (Clean Architecture + CQRS)              |
| `roadmap.md`                | Roadmap completo de entregas e planejamento de produto                         |
| `CHANGELOG.md`              | Histórico de versões e mudanças implementadas                                  |

## Contexto de Negócio

**BabaPlay** é uma plataforma SaaS multi-tenant voltada para organização de peladas. Cada **tenant** representa um grupo/associação que possui seus próprios jogadores, times, partidas e configurações financeiras. O sistema suporta:

- Gestão de jogadores e posições
- Organização de game days e partidas
- Controle de check-in com geolocalização
- Sistema financeiro (mensalidades e caixa)
- Ranking de jogadores baseado em eventos de partida
- Convites de associação por e-mail
- Notificações em tempo real via SignalR
- Geração de PDF de súmula de partidas

## Stack Tecnológica

- **Backend:** .NET 10, C#, PostgreSQL, Entity Framework Core, CQRS, SignalR, JWT
- **Frontend (novo):** Blazor WebAssembly (.NET 10) — em migração ativa
- **Frontend (legado):** React 18 + TypeScript + Vite (pasta `web/`)
- **Infraestrutura:** Docker, Docker Compose, Nginx, Cloudinary, Resend (e-mail)

## Regras para LLM ao atuar nesta pasta

1. **Nunca editar** `agents.md` sem instrução explícita do usuário — ele contém as regras de comportamento da IA.
2. **Consultar** `blazor_functions_guide.md` para entender o estado atual de migração antes de criar ou modificar qualquer componente Blazor.
3. **Consultar** `functions.md` para mapear quais funcionalidades já existem antes de propor novas.
4. **Respeitar** a separação entre `Backend/` (C#/.NET) e `web/` (React legado); não misturar lógica entre eles.
5. O diretório `web/` é legado — preferir implementar novas funcionalidades no projeto Blazor em `Backend/src/BabaPlay.Web/`.
