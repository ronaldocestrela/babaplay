# BabaPlay Front-End Login Guide

Este documento descreve as mudanças de login que acompanham a implementação de
multi-tenancy transparente no back-end. Ele deve ser entregue às equipes de
front-end (web e mobile) para orientar o desenvolvimento.

---

## 🔐 Fluxos de Autenticação

### 1. Mobile / Header (Legado)

- **Endpoint**: `POST /api/token/login`
- **Uso**: apps nativos ou clientes que já enviam cabeçalho `tenant`.
- **Body**:
  ```json
  {
    "username": "user@tenant.com",
    "password": "P@ssw0rd"
  }
  ```
- **Header obrigatórios**:
  ```http
  tenant: <slug-do-tenant>
  Content-Type: application/json
  ```
- **Resposta**: igual ao retorno anterior (`{ data: { jwt, refreshToken, ... } }`).

### 2. Web / Subdomínio (Novo)

- **Endpoint**: `POST /api/token/login-web`
- **Uso**: SPAs ou sites hospedados sob um subdomínio de `babaplay.com`.
- **Tenant é inferido pelo host**; nenhum cabeçalho extra é enviado.
- **URL de exemplo**:
  - Desenvolvimento: `http://root.localhost:5148/api/token/login-web`
  - Produção: `https://minhaassociacao.babaplay.com/api/token/login-web`
- **Body**: mesmo que acima.

> **Observação**: o backend vincula a estratégia de host conforme a variável de
> configuração `Tenancy:HostTemplate`.

---

## 🛠️ Configuração do Cliente

### Mobile / Legacy

```js
await fetch('/api/token/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'tenant': 'minha-associacao'
  },
  body: JSON.stringify({ username, password })
});
```

### Web

```js
// Ajuste base URL de acordo com o tenant
const response = await fetch('/api/token/login-web', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});
```

Em dev local configure o `hosts` para mapear `127.0.0.1 root.localhost` etc.
E instrua a ferramenta de dev (`webpack-dev-server`, `vite`, etc.) a escutar em
`0.0.0.0` para aceitar requisições dirigidas a sub‑domínios.

---

## 📝 Notas adicionais

- **CORS** já permite `https://*.babaplay.com` dinamicamente.
- Emails não são únicos globalmente; uma mesma pessoa pode pertencer a vários
  tenants.
- Endpoint antigo (`/login`) pode ser mantido durante migração.
- JWT deve ser guardado normalmente e enviado em `Authorization: Bearer …`.

---

## ✅ Checklist para Rollout

1. Atualizar componentes de login (web/mobile).
2. Documentar novo fluxo no README e wiki interno.
3. Testar em ao menos dois tenants.
4. Configurar DNS e wildcard SSL em produção.
5. Atualizar testes e scripts de E2E.

---

Qualquer dúvida ou ajuda adicional, estou à disposição!
