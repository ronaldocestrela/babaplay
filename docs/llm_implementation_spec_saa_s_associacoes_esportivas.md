# BabaPlay
## SaaS de Gestão de Associações Esportivas (MVP)

---

## 🎯 Objetivo
Este documento define **EXATAMENTE como uma LLM deve implementar o sistema**.

Stack obrigatória:
- .NET 10
- ASP.NET Core Web API
- SQL Server
- Entity Framework Core (Code First)
- ASP.NET Core Identity
- Multitenancy com isolamento por banco

---

## 🧱 1. Estrutura do Projeto

Criar solução com a seguinte estrutura:

```
src/
  BuildingBlocks/
    Core/
    Infrastructure/
    Security/

  Modules/
    Platform/
    Identity/
    Associations/
    Associates/
    CheckIns/
    TeamGeneration/
    Financial/

  WebApi/
```

---

## 🧩 2. BaseEntity (OBRIGATÓRIO)

Todas as entidades devem herdar de BaseEntity.

```csharp
public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

---

## 🧩 3. BaseController (OBRIGATÓRIO)

```csharp
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success(object data)
        => Ok(new { success = true, data });

    protected IActionResult Error(string message)
        => BadRequest(new { success = false, message });

    protected string GetUserId()
        => User?.FindFirst("sub")?.Value;
}
```

---

## 🔐 4. Segurança

### CORS (dinâmico via banco)
- Criar tabela: AllowedOrigins
- Carregar origens via middleware

### Autenticação
- JWT Bearer
- Identity + Claims

### Autorização
- Policy baseada em permissões

---

## 🏢 5. Multitenancy

### Banco central
- Tenants
- Plans
- Subscriptions

### Banco por tenant
- Todos os dados da associação

### Identificação
- Subdomínio

Criar middleware:
- Resolve tenant
- Injeta connection string

---

## 💰 6. Ordem de Implementação (CRÍTICO)

### FASE 1 - Fundação
1. Criar solução
2. BaseEntity
3. BaseController
4. Configurar EF Core
5. Configurar Identity
6. JWT Auth
7. CORS dinâmico

---

### FASE 2 - Plataforma (Backoffice)

#### Entidades
- Plan
- Tenant
- Subscription

#### Funcionalidades
- CRUD Plans
- CRUD Tenants
- Contratação (cria banco automático)

---

### FASE 3 - Tenant Setup

#### Entidades
- Association

Campos:
- Name
- Address
- Regulation (opcional)
- PlayersPerTeam — número alvo de jogadores por equipa na geração de times (ex.: 5 futsal, 11 futebol); mínimo 2; default 5

---

### FASE 4 - Identity

#### ApplicationUser
- herdar IdentityUser<string>

Campos extras:
- UserType
- AssociateId

---

### FASE 5 - Permissões

#### Entidades
- Role
- Permission
- RolePermission

Criar:
- Seed de permissões padrão

---

### FASE 6 - Associados

#### Entidades
- Associate
- Position
- AssociatePosition

Regras:
- mínimo 1 posição
- máximo 3

---

### FASE 7 - Mensalidades

#### Entidades
- Membership
- Payment

---

### FASE 8 - Check-in

#### Entidades
- CheckInSession
- CheckIn

Regra:
- 1 check-in por dia por associado

---

### FASE 9 - Times

#### Entidades
- Team
- TeamMember

#### Serviço
- TeamGenerationService

Algoritmo:
1. Ordenar associados pela **primeira** hora de check-in na sessão (ordem de chegada).
2. Ler **PlayersPerTeam** da associação no tenant (default 5 se não existir registo).
3. Calcular número de equipas: `max(2, totalDeAssociadosComCheckIn / PlayersPerTeam)` (divisão inteira).
4. Distribuir associados em **round-robin** pelas equipas (excedentes repartidos pelos primeiros times).

---

### FASE 10 - Financeiro

#### Entidades
- CashEntry
- Category

---

## 🧠 7. Regras de Negócio

- Tenant isolado por banco
- 1 check-in por dia
- associado precisa de posição
- roles configuráveis
- geração de times: `PlayersPerTeam` na associação (mínimo 2); número de equipas derivado dos check-ins da sessão (ver FASE 9)

---

## 🚀 8. Requisitos Técnicos

- Usar EF Core Code First
- Migrations automáticas por tenant
- Dependency Injection
- Services separados por módulo

---

## 📌 9. Entrega Esperada

A LLM deve gerar:

- Controllers
- Services
- Entities
- DbContexts
- Migrations
- Middlewares

Tudo funcional.

---

## ⚠️ IMPORTANTE

- NÃO criar microservices
- NÃO complicar arquitetura
- Priorizar MVP funcional

---

## FIM

