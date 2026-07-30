# 📁 FOLDER_CONTEXT — web/ (Frontend React — Legado)

## Instrução para LLM

Você está no diretório **web/**. Este é o **frontend legado do BabaPlay**, construído em React 18 + TypeScript com Vite. **Este frontend está em processo de migração para Blazor WebAssembly** (`Backend/src/BabaPlay.Web/`).

## ⚠️ ATENÇÃO: STATUS DE MIGRAÇÃO

> Este projeto **NÃO deve receber novas funcionalidades**. Toda nova feature deve ser implementada diretamente no projeto Blazor em `Backend/src/BabaPlay.Web/`.
> 
> Consulte `blazor_functions_guide.md` na raiz para entender o estado atual da migração.

## Responsabilidade deste projeto

Serviu como (e ainda serve temporariamente) a interface principal do BabaPlay enquanto o frontend Blazor não está completamente pronto:

| Diretório / Arquivo | Responsabilidade                                                                    |
|---------------------|-------------------------------------------------------------------------------------|
| `src/features/`     | Módulos de funcionalidade organizados por domínio (auth, players, teams, matches, etc.) |
| `src/core/`         | Infraestrutura do frontend: cliente HTTP, providers, hooks, tipos globais           |
| `src/pages/`        | Páginas de nível de rota (mapeadas pelo React Router)                               |
| `src/layouts/`      | Layouts de página (sidebar, navbar, estrutura geral)                                |
| `src/assets/`       | Assets estáticos (imagens, SVGs, ícones)                                            |
| `src/router.tsx`    | Definição de rotas do React Router com lazy loading                                  |
| `src/App.tsx`       | Componente raiz da aplicação React                                                  |
| `nginx/`            | Configuração do Nginx para servir o SPA em produção via Docker                      |
| `dist/`             | Build de produção gerado pelo Vite (não versionar)                                  |

## Estrutura de Features (módulos de domínio)

| Feature                | Responsabilidade                                              |
|------------------------|---------------------------------------------------------------|
| `auth/`                | Login, logout, contexto de autenticação, fluxo de onboarding  |
| `players/`             | Gestão de jogadores (listagem, detalhe, criação, edição)      |
| `teams/`               | Gestão de times                                               |
| `matches/`             | Gestão de partidas e eventos em tempo real                    |
| `checkin/`             | Fluxo de check-in com geolocalização                          |
| `dashboard/`           | Dashboard principal                                           |
| `positions/`           | Gestão de posições de futebol                                 |
| `tenant-settings/`     | Configurações do tenant                                       |
| `tenant-onboarding/`   | Fluxo de criação de novo tenant                               |
| `tenant-invitations/`  | Gestão de convites de associação                              |

## Estrutura de Core (infraestrutura frontend)

| Pasta          | Responsabilidade                                              |
|----------------|---------------------------------------------------------------|
| `api/`         | Configuração do Axios e funções de chamada à API REST         |
| `components/`  | Componentes React reutilizáveis (botões, modais, tabelas)     |
| `providers/`   | Context providers (autenticação, tenant, toast)               |
| `services/`    | Serviços de negócio frontend                                  |
| `types/`       | TypeScript types e interfaces globais                         |
| `utils/`       | Funções utilitárias (formatação, validação, etc.)             |
| `constants/`   | Constantes da aplicação (URLs, chaves, etc.)                  |

## Regras para LLM ao atuar neste projeto

1. **Não adicionar novas features** — redirecionar implementação para o projeto Blazor.
2. **Apenas correções críticas de bugs** são aceitas neste projeto durante a fase de migração.
3. Para executar em desenvolvimento: `yarn dev` ou `npm run dev` na pasta `web/`.
4. Build de produção: `yarn build` — gera o diretório `dist/`.
5. O Nginx em `nginx/` serve o SPA e redireciona todas as rotas para `index.html` (suporte a React Router).
6. **Variáveis de ambiente** seguem o padrão Vite: prefixo `VITE_` e arquivo `.env` (baseado em `.env.example`).
