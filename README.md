# BabaPlay 🚀

**BabaPlay** é uma API Web em .NET 8 focada em gestão multi-tenant de associações e associados (português). O projeto já inclui identidade, autenticação por JWT, permissões por claims, documentação OpenAPI e suporte a migrações/seed de banco.

---

## 🧩 Principais Recursos

- **Multi-tenant** (Finbuckle.MultiTenant) com header/claim de tenant
- **Autenticação JWT** e integração com **ASP.NET Identity**
- **Permissions/Policies** por claims e roles
- **CQRS + MediatR** para separação de responsabilidades
- **Validação com FluentValidation** e pipeline de validação
- **OpenAPI (NSwag)** com suporte a autenticação via JWT
- **Entity Framework Core** com migrations e seeders
- **Middleware** para tratamento de erros e usuário corrente

---

## 🏗️ Arquitetura

Projeto organizado em camadas:

- `Domain` — entidades e modelos de domínio
- `Application` — regras de negócio, CQRS (commands/queries), validações
- `Infrastructure` — EF Core, Identity, OpenAPI, multi-tenant, serviços de infraestrutura
- `WebApi` — ponto de entrada, controllers e configuração do pipeline

---

## ⚙️ Requisitos

- .NET 8 SDK
- Docker (opcional, recomendado para SQL Server)

---

## ▶️ Execução rápida

1. Inicie o SQL Server (com Docker):

```bash
docker-compose up -d
```

> Arquivo: `docker-compose.yml` já traz uma configuração de SQL Server usada nas configurações padrão.

2. Restaurar e compilar:

```bash
dotnet restore
dotnet build
```

3. Aplicar migrations / inicializar banco:

```bash
dotnet ef database update --project Infrastructure --startup-project WebApi --context ApplicationDbContext
```

4. Rodar a API:

```bash
dotnet run --project WebApi
```

5. Abrir a documentação OpenAPI (Swagger):

Acesse `https://localhost:{PORT}/swagger` (normalmente `https://localhost:5001/swagger`).

---

## 🔐 Endpoints úteis

- **Login**: `POST /api/token/login` (perceba que o endpoint exige o header `tenant`)
  - Body:

```json
{
  "username": "admin.root@babaplay.com.br",
  "password": "P@ssw0rd@123"
}
```
  - Header: `tenant: root`

- **Refresh token**: `POST /api/token/refresh-token`
- **Tenants management**: `POST /api/tenants/add`, `PUT /api/tenants/{id}/activate` etc.
- **Users**: `POST /api/users/register`, `GET /api/users/all`, etc.

> Use o Swagger para explorar todos os endpoints e testar com JWT.

---

## 🛠️ Configurações importantes

- `appsettings.json` contém `ConnectionStrings:DefaultConnection` e `JwtSettings` (secret & tempos de expiração).
- Valores padrão presentes no código para facilitar desenvolvimento:
  - Tenant `root` — `admin.root@babaplay.com.br` / senha padrão em `Infrastructure/Tenancy/TenancyConstants.cs`.

> ⚠️ **Atenção:** altere secrets (JWT secret, senhas, connection strings) antes de ir para produção.

---

## 💡 Dicas de Desenvolvimento

- Validações são feitas com FluentValidation e aplicadas automaticamente pelo pipeline (veja `Application/Pipelines`).
- Permissões baseadas em claims são configuradas via políticas no startup de infraestrutura.
- Para adicionar novas migrations use `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi --context ApplicationDbContext`.

---

## Contribuição

Contribuições são bem-vindas! Abra issues ou PRs explicando a mudança e seguindo o padrão do projeto.

---

## 📄 Licença & Contato

- Licença: **MIT** (conforme `SwaggerSettings`)
- Contato: `info@babaplay.com.br`

---

Se quiser, posso: adicionar um arquivo `docker-compose.override` para dev, configurar um README com badges CI, ou incluir exemplos de requests mais detalhados. ✅