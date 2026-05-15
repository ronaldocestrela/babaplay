# Deploy Manual com Docker (sem CD)

Este guia descreve como subir o BabaPlay manualmente com Docker Compose, sem pipeline de CI/CD.

## Escopo

- Backend: .NET 10 API
- Frontend: React + Vite servido por Nginx
- Banco: SQL Server 2022 em container
- Ambiente inicial: HTTP (sem HTTPS)

## Arquivos de deploy

- `deploy/docker/docker-compose.manual.yml`
- `deploy/docker/.env.manual.example`
- `Backend/Dockerfile`
- `Backend/.dockerignore`
- `web/Dockerfile`
- `web/.dockerignore`
- `web/nginx/default.conf`

## Pre-requisitos

1. Docker Engine 24+ e Docker Compose v2.
2. Portas livres no host:
   - 8080 (web)
   - 5050 (api)
  - 1433 (sqlserver, ou valor configurado em `SQL_EXTERNAL_PORT`)
3. Minimo de 4 GB de RAM livre para containers.

## 1) Configurar variaveis de ambiente

Na raiz do projeto, copie o template:

```bash
cp deploy/docker/.env.manual.example deploy/docker/.env.manual
```

Edite o arquivo `deploy/docker/.env.manual` com valores reais, principalmente:

- `SQL_EXTERNAL_PORT` (porta externa do SQL no host; default: `1433`)
- `SQL_SA_PASSWORD`
- `MASTER_DB_CONNECTION_STRING`
- `JWT_SECRET_KEY`
- `CORS_ALLOWED_ORIGIN`
- `WEB_VITE_API_URL`
- `RESEND_API_KEY` (se houver envio real de e-mails)
- `TENANT_LOGO_STORAGE_PROVIDER` (`Local` ou `Cloudinary`)
- `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`
- `CLOUDINARY_FOLDER`

Observacoes:

- `SQL_EXTERNAL_PORT` altera apenas a porta exposta no host (`<host>:1433`).
- `MASTER_DB_CONNECTION_STRING` deve apontar para `sqlserver,1433` quando usar o compose fornecido.
- `WEB_VITE_API_URL` e injetada no build do frontend. Para este stack: `http://localhost:5050`.
- `CORS_ALLOWED_ORIGIN` deve bater com a URL publica da web. Para este stack: `http://localhost:8080`.
- Para ativar Cloudinary no logo da associacao, defina `TENANT_LOGO_STORAGE_PROVIDER=Cloudinary` e preencha as variaveis `CLOUDINARY_*`.

## 2) Build das imagens

```bash
docker compose \
  -f deploy/docker/docker-compose.manual.yml \
  --env-file deploy/docker/.env.manual \
  build
```

## 3) Subir stack

```bash
docker compose \
  -f deploy/docker/docker-compose.manual.yml \
  --env-file deploy/docker/.env.manual \
  up -d
```

## 4) Validar servicos

Verificar estado:

```bash
docker compose -f deploy/docker/docker-compose.manual.yml ps
```

Logs da API:

```bash
docker compose -f deploy/docker/docker-compose.manual.yml logs -f api
```

Acessos esperados:

- Web: `http://localhost:8080`
- Swagger: `http://localhost:5050/swagger`
- Readiness da API: `http://localhost:5050/api/v1/ping`

Exemplo de validacao de readiness:

```bash
curl -i http://localhost:5050/api/v1/ping
```

Resultado esperado:

- `200 OK` com `{"status":"healthy",...}` quando banco master estiver acessivel.
- `503 Service Unavailable` com `{"status":"unhealthy",...}` quando banco master estiver indisponivel.

## 5) Migrations (master DB)

A API executa `masterDb.Database.Migrate()` no startup (exceto em ambiente Testing).

Implicacao operacional:

- Se o SQL Server estiver indisponivel, a API pode falhar ao iniciar.
- Sempre aguarde `sqlserver` ficar healthy antes de validar a API.

## 6) Atualizacao manual (rolling simples)

Quando houver nova versao de codigo:

```bash
docker compose \
  -f deploy/docker/docker-compose.manual.yml \
  --env-file deploy/docker/.env.manual \
  build

docker compose \
  -f deploy/docker/docker-compose.manual.yml \
  --env-file deploy/docker/.env.manual \
  up -d
```

## 7) Rollback manual

Sem CD, o rollback e feito por imagem/tag anterior (se publicada) ou checkout manual do commit anterior e novo build:

```bash
git checkout <commit-anterior>
docker compose -f deploy/docker/docker-compose.manual.yml --env-file deploy/docker/.env.manual build
docker compose -f deploy/docker/docker-compose.manual.yml --env-file deploy/docker/.env.manual up -d
```

## 8) Parada e limpeza

Parar mantendo dados:

```bash
docker compose -f deploy/docker/docker-compose.manual.yml down
```

Parar removendo volumes (apaga banco e storage):

```bash
docker compose -f deploy/docker/docker-compose.manual.yml down -v
```

## 9) Persistencia

O compose define volumes nomeados:

- `babaplay_sql_data`: dados do SQL Server
- `babaplay_api_storage`: artefatos locais da API (ex.: sumulas)

Sem `down -v`, os dados persistem entre reinicios.

## 10) Troubleshooting rapido

1. API nao sobe:
   - confira `MASTER_DB_CONNECTION_STRING`
   - valide senha do `sa` e politica de senha forte do SQL Server
  - valide readiness: `curl -i http://localhost:5050/api/v1/ping`
2. Frontend sem comunicar com API:
   - confirme `WEB_VITE_API_URL` no arquivo de ambiente
   - confirme `CORS_ALLOWED_ORIGIN`
3. SQL Server unhealthy:
   - aguarde mais tempo no primeiro startup
   - verifique logs: `docker compose -f deploy/docker/docker-compose.manual.yml logs sqlserver`
