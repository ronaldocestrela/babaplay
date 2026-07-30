# 📁 FOLDER_CONTEXT — deploy/

## Instrução para LLM

Você está no diretório **deploy/**. Esta pasta contém todas as configurações necessárias para fazer o deploy do BabaPlay via Docker Compose.

## Responsabilidade desta pasta

Centraliza os artefatos de deploy e infraestrutura containerizada do sistema:

| Arquivo / Diretório              | Responsabilidade                                                              |
|----------------------------------|-------------------------------------------------------------------------------|
| `docker/`                        | Subdiretório com configurações Docker Compose por ambiente                    |
| `docker/docker-compose.manual.yml` | Composição completa para deploy manual em servidor VPS/self-hosted          |
| `docker/`.env`                   | Variáveis de ambiente para o docker-compose (secrets, URLs, configurações)   |
| `docker/.env.manual.example`     | Arquivo de exemplo com todas as variáveis necessárias para deploy manual      |

## Serviços orquestrados pelo Docker Compose

O `docker-compose.manual.yml` provavelmente orquestra:
- **babaplay-api** — Container da API .NET (imagem gerada pelo `Backend/Dockerfile`)
- **babaplay-web** — Container do frontend Blazor/React (imagem gerada pelos Dockerfiles de `web/`)
- **postgres** — Banco de dados PostgreSQL
- **nginx** — Proxy reverso para expor os serviços (API + frontend)

## Documentação relacionada

- `docs/deploy-manual-docker.md` — Guia completo passo-a-passo para deploy manual com Docker

## Regras para LLM ao atuar nesta pasta

1. **Nunca commitar** o arquivo `docker/.env` com secrets reais — usar o `.env.manual.example` como referência.
2. Ao adicionar um novo serviço ao sistema, criar o serviço correspondente no `docker-compose.manual.yml`.
3. **Variáveis de ambiente sensíveis** (senhas, tokens, API keys) devem estar em `.env` e nunca hardcoded nos arquivos YAML.
4. Consultar `docs/deploy-manual-docker.md` antes de modificar a configuração de deploy.
5. Para executar o ambiente de forma manual, usar o script `scripts/ci-local.sh` na raiz do projeto.
