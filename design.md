# Design UI/UX — BabaPlay Web (Blazor WASM)

Documento de referência para decisões visuais, componentes reutilizáveis e padrões de interface do frontend ativo em `Backend/src/BabaPlay.Web/`.

> **Frontend legado:** o projeto React em `web/` está em migração e **não** deve receber novas features de UI. Toda evolução de design aplica-se ao Blazor.

---

## 1. Princípios

| Princípio | Descrição |
|---|---|
| **Tema escuro por padrão** | Área autenticada usa fundo `--bg-dark` / `--bg-shell` com cards em `--bg-card`. |
| **Tokens CSS primeiro** | Cores, sombras e raios vivem em `:root` em `wwwroot/css/app.css`. Evitar cores hardcoded em componentes. |
| **Sem estilos inline** | Preferir classes do design system (`.widget-card`, `.page-header`, etc.) ou utilitários Bootstrap. |
| **Ícones consistentes** | Usar **Bootstrap Icons** (`bi bi-*`) em telas autenticadas. Evitar emojis em UI de produção. |
| **Mobile-first no shell** | Sidebar off-canvas abaixo de 992px; conteúdo nunca deve depender de sidebar fixa em telefone. |
| **Feedback de carregamento** | Skeleton no carregamento inicial; spinner/opacity no refresh; nunca “piscar” layout vazio → conteúdo. |
| **Empty states padronizados** | Usar o componente `EmptyState` com ícone, título, dica e ação opcional. |

---

## 2. Stack de estilos

### 2.1 Arquivos e ordem de cascade

Host: [`Backend/src/BabaPlay.Web/wwwroot/index.html`](Backend/src/BabaPlay.Web/wwwroot/index.html)

Ordem de carregamento:

1. **Bootstrap 5.3.3** (CDN) — grid, botões, modais, tabelas, utilitários.
2. **Bootstrap Icons 1.11.3** (CDN) — ícones `bi bi-*`.
3. **Tailwind CSS** (CDN, runtime) — páginas públicas de auth/onboarding.
4. **`app.css`** (local, por último) — tokens BabaPlay e overrides do tema escuro.

Configuração Tailwind no host:

```javascript
tailwind.config = {
  corePlugins: { preflight: false, container: false },
};
```

- `preflight: false` evita reset conflitando com Bootstrap/`app.css`.
- `container: false` evita colisão com a classe Bootstrap `.container`.

### 2.2 Zonas da aplicação

| Zona | Layout | Framework predominante |
|---|---|---|
| **Autenticada** | `MainLayout` (sidebar + header + content) | Bootstrap + `app.css` |
| **Pública** | `PublicLayout` (container centrado) | Tailwind + `app.css` |
| **404 / redirect login** | `PublicLayout` | `app.css` + `.public-card` |

### 2.3 Dívida técnica conhecida

- Bootstrap e Tailwind via **CDN** (atalho temporário). Roadmap: consolidar em **um** framework com build próprio (npm/Tailwind CLI ou pacote Bootstrap local).
- Coexistem classes legadas Bootstrap 4 (`font-weight-bold`, `mr-2`) — aliases em `app.css` mantêm compatibilidade.
- Algumas páginas internas ainda usam estilos inline ou empty states ad hoc (ver seção 10).

---

## 3. Design tokens

Definidos em [`wwwroot/css/app.css`](Backend/src/BabaPlay.Web/wwwroot/css/app.css):

| Token | Valor | Uso |
|---|---|---|
| `--primary-color` | `#0d9488` | Ações primárias, destaques |
| `--primary-hover` | `#0f766e` | Hover/active de botões primários |
| `--primary-light` | `#2dd4bf` | Ícones, subtítulos, gradiente |
| `--bg-dark` | `#0f172a` | Fundo do body |
| `--bg-shell` | `#020617` | Sidebar, header |
| `--bg-card` | `#1e293b` | Cards e widgets |
| `--bg-secondary` | `#334155` | Progress bars, skeleton |
| `--text-main` | `#f8fafc` | Texto principal |
| `--text-muted` | `#94a3b8` | Texto secundário |
| `--border-color` | `#334155` | Bordas de cards e divisores |
| `--accent-gold` | `#f59e0b` | Destaques pontuais |
| `--danger` | `#ef4444` | Erros, ações destrutivas |
| `--success` | `#22c55e` | Sucesso, RSVP confirmado |
| `--shadow-card` | `0 4px 12px rgba(0,0,0,0.25)` | Elevação padrão |
| `--radius-md` | `10px` | Cards |
| `--radius-lg` | `12px` | Cards públicos |

### Gradiente de título

Classe `.text-gradient` — gradiente teal aplicado a títulos de página (`Painel de Controle`, listagens, etc.).

---

## 4. Tipografia

- **Família:** `system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, …`
- **Título de página:** `.page-header__title` + `.text-gradient` (1.25rem, bold)
- **Subtítulo de página:** `.page-header__subtitle` (0.9rem, `--text-muted`)
- **Título de widget:** `.widget-card__title` (1rem, bold, ícone `.bi` em `--primary-light`)
- **Valor KPI:** `.stat-card__value` (1.75rem, bold)
- **Aliases legados:** `.font-weight-bold`, `.font-weight-medium`

---

## 5. Layouts

### 5.1 MainLayout (autenticado)

Arquivo: [`Components/Layout/MainLayout.razor`](Backend/src/BabaPlay.Web/Components/Layout/MainLayout.razor)

```
┌─────────────┬──────────────────────────────────┐
│  NavMenu    │  Header (tenant, user, logout) │
│  (sidebar)  ├──────────────────────────────────┤
│             │  content-body (@Body)            │
└─────────────┴──────────────────────────────────┘
```

- **Desktop (≥992px):** sidebar fixa 260px, sempre visível.
- **Mobile (<992px):** sidebar off-canvas; botão hambúrguer no header; backdrop clicável; menu fecha ao navegar ou clicar no backdrop.

Componentes relacionados:

| Arquivo | Responsabilidade |
|---|---|
| `NavMenu.razor` | Links de navegação agrupados; classe `.open` no mobile |
| `Header.razor` | Tenant, notificações, usuário, logout, toggle menu |
| `app.css` | `.sidebar`, `.nav-group`, `.sidebar-backdrop`, `.sidebar-toggle`, media queries |

**Grupos do menu (sidebar):** itens top-level ficam Dashboard, Jogadores, Partidas & RSVP, Sorteio de Times e Configurações. Subitens ficam em secções expansíveis **Financeiro** (Painel, Faturas, Inadimplência, Balancete, Caixinha) e **Comunicação** (Comunicados, Enquetes, Alertas, Chat). O grupo abre automaticamente quando a rota actual pertence a ele.

### 5.2 PublicLayout (público)

Arquivo: [`Components/Layout/PublicLayout.razor`](Backend/src/BabaPlay.Web/Components/Layout/PublicLayout.razor)

- Renderiza apenas `@Body` dentro de `.public-container` (gradiente escuro, padding).
- **Não** envolve conteúdo em `.public-card` — cada página pública define seu próprio card (evita card-duplo).
- Exceção: página 404 em `App.razor` usa `.public-card` explicitamente.

### 5.3 Páginas públicas (auth/onboarding)

Usam Tailwind inline (`bg-slate-900`, `rounded-2xl`, `from-emerald-400`, etc.):

- `Pages/Auth/Login.razor`
- `Pages/Auth/ForgotPassword.razor`
- `Pages/Auth/ResetPassword.razor`
- `Pages/Tenant/RegisterAssociation.razor`
- `Pages/Tenant/AcceptInvite.razor`
- `Pages/Tenant/SelectTenant.razor`
- `Pages/Players/CompleteProfile.razor`

Padrão visual: fundo escuro, card glass (`bg-slate-800/80`), borda `border-slate-700`, gradiente emerald/teal no título.

---

## 6. Componentes do design system

### 6.1 Cards e widgets

| Classe / componente | Uso |
|---|---|
| `.widget-card` | Container de widget do dashboard (borda, sombra, hover) |
| `.widget-card__header` / `__title` | Cabeçalho com ícone Bootstrap |
| `.stat-card` | KPI individual (grid de métricas) |
| `.stats-grid` | Grid responsivo `repeat(auto-fit, minmax(200px, 1fr))` |
| `.card` / `.card-custom` | Cards Bootstrap com override de tema escuro em `.content-body` |

### 6.2 Cabeçalho de página

```html
<div class="page-header">
  <div>
    <h1 class="page-header__title text-gradient">Título</h1>
    <p class="page-header__subtitle">Descrição</p>
  </div>
  <!-- ações (botões) -->
</div>
```

Referência: [`Pages/Dashboard/Dashboard.razor`](Backend/src/BabaPlay.Web/Pages/Dashboard/Dashboard.razor)

### 6.3 EmptyState

Arquivo: [`Components/Common/EmptyState.razor`](Backend/src/BabaPlay.Web/Components/Common/EmptyState.razor)

| Parâmetro | Descrição |
|---|---|
| `IconClass` | Classe Bootstrap Icon (ex: `bi-calendar-x`) |
| `Title` | Mensagem principal |
| `Hint` | Texto de orientação (opcional) |
| `TestId` | `data-testid` para testes |
| `IsPageLevel` | Se `true`, envolve em `.widget-card.page-empty-card` |
| `ChildContent` | Botões de ação (ex: “Agendar primeira partida”) |

**Onde já está adotado:**

- Dashboard: `NextMatchWidget`, `RecentAnnouncementsWidget`
- `MatchList`, `PlayerList`, `TeamDraw`
- Financeiro: `InvoicesList`, `DefaultersReport`, `FinancialStatement`, `Fundraisers`
- Comunicação: `Announcements`, `Polls`, `Notifications`, `TeamChat`
- Settings: `TenantSettings` (e widgets de aba)
- `RecentTransactionsWidget`, `NotificationCenter` (dropdown)
- `TeamColumn` (empty state interno quando o time não tem jogadores)

### 6.4 Loading

| Componente / classe | Quando usar |
|---|---|
| `LoadingSpinner` | Spinner centralizado com mensagem (listagens, widgets isolados) |
| `DashboardSkeleton` | Carregamento inicial do dashboard (layout espelhado) |
| `.skeleton` + variantes | Placeholders animados (shimmer) |
| `.dashboard-content--refreshing` | Opacity reduzida durante refresh (mantém conteúdo visível) |

**Padrão de refresh (Dashboard):**

- Botão desabilitado + texto “Atualizando...” + spinner.
- Conteúdo existente permanece visível com opacidade ~55%.

### 6.5 Badges

Classes custom em `app.css` (não confundir com `.badge` Bootstrap puro):

- `.badge-primary`, `.badge-success`, `.badge-warning`, `.badge-danger`
- `.badge-secondary`, `.badge-info`, `.badge-light`

Semântica de status (ex.: RSVP): success = confirmado, danger = recusado, warning = lista de espera, light = não respondido.

### 6.6 Modais

Padrão Bootstrap manual (`modal-backdrop` + `modal fade show`), sem JS Bootstrap Modal.

Modais longos devem incluir:

```html
<div class="modal-dialog modal-dialog-centered modal-lg modal-dialog-scrollable modal-fullscreen-sm-down">
```

Adotado em: `MatchModal`, `EditPlayerModal`, `CreatePollModal`, `CreateAnnouncementModal`, `CreateInvoiceModal`, `PixPaymentModal`, `CreateFundraiserModal`, `ContributeFundraiserModal`.

Estrutura de referência (tema escuro via `.app-modal` / `.app-modal-backdrop` em `app.css`):

```html
<div class="modal-backdrop fade show app-modal-backdrop"></div>
<div class="modal fade show d-block app-modal" tabindex="-1">
  <div class="modal-dialog modal-dialog-centered modal-lg modal-dialog-scrollable modal-fullscreen-sm-down">
    <div class="modal-content">...</div>
  </div>
</div>
```

- Fechar: `btn-close btn-close-white` com `aria-label="Fechar"`.
- Labels: `.filters-toolbar__label`.
- Sem estilos inline nem emojis no título/ações.

---

## 7. Responsividade

### 7.1 Breakpoints

| Breakpoint | Alinhamento | Comportamento principal |
|---|---|---|
| `< 576px` | Bootstrap `sm` | Header compacto; nomes truncados; prancheta tática menor |
| `< 992px` | Bootstrap `lg` | Sidebar off-canvas; padding `content-body` 1rem |
| `≥ 992px` | Desktop | Sidebar fixa; padding 2rem |

### 7.2 Padrões mobile implementados

| Área | Solução |
|---|---|
| Shell | Sidebar off-canvas + hambúrguer + backdrop |
| Tabelas densas | `table-responsive` + colunas ocultas (`d-none d-md-table-cell`) |
| NotificationCenter | `width: min(340px, calc(100vw - 2rem))` |
| ScoreboardWidget | Colunas empilhadas + `clamp()` no placar |
| TacticalBoard | Pins e altura reduzidos em ≤576px |
| MatchList filtros | `.match-list-filters { flex-wrap: wrap }` |
| TeamDraw | Opções em `.widget-card.filters-toolbar`; colunas empilhadas em `<md` |
| Financeiro | Tabelas `.financial-table` + colunas `d-none d-md/lg-table-cell`; filtros em toolbar |
| TeamChat | Altura `calc(100dvh - header - padding)`; bolhas `.team-chat-bubble` |

### 7.3 Testes visuais recomendados

Em DevTools, validar em **360px**, **390px**, **768px** e **≥992px** após mudanças de layout.

---

## 8. Ícones

- **Biblioteca oficial:** Bootstrap Icons (`<i class="bi bi-nome" aria-hidden="true"></i>`).
- **NavMenu:** usa Bootstrap Icons (`bi-*`) nos links e toggles de grupo.
- **StatCard:** alguns KPIs ainda usam emoji no parâmetro `Icon` — preferir ícones Bootstrap no futuro.

---

## 9. Acessibilidade

| Item | Estado |
|---|---|
| Viewport meta | ✅ `width=device-width, initial-scale=1.0` |
| Botão menu | ✅ `aria-label`, `aria-expanded` |
| Ícones decorativos | ✅ `aria-hidden="true"` nos `.bi` de título |
| Empty states | ✅ Título em texto (não só ícone) |
| Foco visível | ⚠️ Depende dos defaults Bootstrap; revisar em componentes custom |
| Contraste | ✅ Texto claro sobre fundo escuro; badges com cores semitransparentes |

---

## 10. Mapa de arquivos relevantes

| Caminho | Conteúdo |
|---|---|
| `Backend/src/BabaPlay.Web/wwwroot/css/app.css` | **Fonte da verdade** — tokens, layout, mobile, design system |
| `Backend/src/BabaPlay.Web/wwwroot/index.html` | CDN Bootstrap/Tailwind, host WASM |
| `Components/Layout/` | MainLayout, PublicLayout, Header, NavMenu |
| `Components/Common/` | EmptyState, StatCard, LoadingSpinner, Badge, ConfirmModal |
| `Components/Dashboard/` | DashboardSkeleton, NextMatchWidget, QuickStatsWidget, RecentAnnouncementsWidget |
| `Pages/Dashboard/Dashboard.razor` | Página raiz `/` — referência de page-header + loading |

---

## 11. Checklist para novas telas

Ao criar ou alterar uma página autenticada:

- [ ] Usar `.page-header` + `.text-gradient` no título
- [ ] Conteúdo principal dentro de `.container-fluid py-4`
- [ ] Cards/widgets com `.widget-card` ou `.card.card-custom` (não estilos inline)
- [ ] Empty state via `<EmptyState />` (widget ou `IsPageLevel="true"`)
- [ ] Loading: skeleton ou spinner; em refresh, não esconder dados sem necessidade
- [ ] Tabelas: `table-responsive`; ocultar colunas secundárias no mobile
- [ ] Modais longos: `modal-fullscreen-sm-down` + `modal-dialog-scrollable`
- [ ] Ícones Bootstrap Icons, não emojis
- [ ] Testar em viewport mobile (sidebar, overflow horizontal)
- [ ] Adicionar `data-testid` em elementos-chave para testes BUnit

---

## 12. Roadmap de design (pendências)

| Prioridade | Item |
|---|---|
| Média | Consolidar CDN → build local (Tailwind CLI ou Bootstrap npm) |
| Média | Migrar StatCard de emojis para Bootstrap Icons |
| Média | Migrar empty states restantes (Rankings, Positions, etc.) para `EmptyState` |
| Média | Aplicar `.page-header` nas demais páginas de listagem (TeamDraw, Financeiro, Comunicação e TenantSettings já migrados) |
| Baixa | PWA (manifest, service worker) — fora de escopo atual |
| Baixa | Tabelas como cards no mobile (alternativa a scroll horizontal) |
| Baixa | Tema claro (dark-only hoje) |

---

## 13. Histórico de decisões

| Data | Decisão |
|---|---|
| 2026-07 | NavMenu: grupos expansíveis Financeiro/Comunicação + ícones Bootstrap (menos itens top-level) |
| 2026-07 | Adicionado Bootstrap + Bootstrap Icons + Tailwind CDN em `index.html` (app estava sem CSS framework) |
| 2026-07 | Implementado shell mobile off-canvas (sidebar 260px quebrava telefone) |
| 2026-07 | Criado design system em `app.css` (tokens, widget-card, page-header, overrides Bootstrap) |
| 2026-07 | Dashboard: skeleton loading, refresh com opacity, EmptyState, elevação de cards |
| 2026-07 | PublicLayout simplificado (removido card-duplo em páginas Tailwind) |
| 2026-07 | TeamDraw (`/teams`): `.page-header`, `EmptyState`, `widget-card` nas opções e colunas; PlayerBadge no tema escuro |
| 2026-07 | Financeiro (`/financial*`): page-header, EmptyState, filtros em widget-card, tabelas `.financial-table`, KPIs sem emoji |
| 2026-07 | Comunicação (`/communication*`): page-header, EmptyState, filtros em pills, cards/chat no tema escuro sem emoji |
| 2026-07 | TenantSettings (`/settings/tenant`): page-header, abas em pills, widgets em `widget-card`, EmptyState nos dias fixos |
| 2026-07 | Modais financeiro/comunicação alinhados a `.app-modal` (scrollable + fullscreen-sm-down) |
| 2026-07 | Aba Dias Fixos (`TenantGameDayOptionsWidget`): page-header local, EmptyState com CTA, tabela/cards mobile |

---

*Última atualização: julho/2026 — manter este arquivo alinhado quando novos padrões visuais forem introduzidos.*
