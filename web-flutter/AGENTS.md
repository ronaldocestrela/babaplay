# Guia geral de agentes — mansao-green-app

> Ponto de entrada para qualquer IDE ou agente de IA.
> Cada camada possui seu próprio `AGENTS.md` com regras detalhadas. Este arquivo é o resumo executivo e o mapa de navegação.

---

## Estrutura de camadas

```
lib/
├── config/      # Bootstrapping, DI, ambientes          → lib/config/AGENTS.md
├── data/        # Repositórios, DTOs, clients, cache     → lib/data/AGENTS.md
├── domain/      # Entidades, use cases, contratos        → lib/domain/AGENTS.md
├── routing/     # Rotas, guards, shells                  → lib/routing/AGENTS.md
└── ui/          # Pages, widgets, view models            → lib/ui/AGENTS.md
```

**Antes de alterar qualquer camada, leia o `AGENTS.md` correspondente.**

---

## Princípios globais

### 1. Dependência unidirecional

```
ui → domain ← data
config (wiring de tudo)
routing → ui
```

- `ui/` nunca importa `data/` diretamente.
- `domain/` não importa Flutter, `ui/`, nem `data/`.
- `data/` não importa `ui/`.
- `routing/` não contém lógica de negócio.

### 2. Retornos funcionais: `Result` / `AsyncResult`

Todo método que pode falhar **retorna `Result<T>` (síncrono) ou `AsyncResult<T>` (assíncrono)**, nunca lança exceção para fluxos controlados.

- `Success(value)` — caminho feliz.
- `Failure(exception)` — falha esperada (negócio, rede, validação).
- Use `fold`, `map`, `flatMap` para composição; evite `if (result.isSuccess)` no lugar de `fold`.

### 3. Preferir parâmetros posicionais

Por padrão, métodos de domínio, data e view models usam **parâmetros posicionais** para permitir method tear-off em `map`/`flatMap`. Named parameters somente quando melhorarem clareza ou segurança.

### 4. DX em `async` — sempre declarar `async` + `await`

Prefira declarar a função como `async` e fazer `await` explícito dentro dela (melhora stack trace):

```dart
// ✅ Preferir
AsyncResult<Unit> execute() async {
  final result = await _repo.step1().flatMap(_repo.step2);
  return result;
}

// ⚠️ Evitar
AsyncResult<Unit> execute() {
  return _repo.step1().flatMap(_repo.step2);
}
```

### 5. Idioma de erro HTTP (`data/`)

```dart
try {
  final response = await _dio.get('/endpoint');
  return Success(/* parse */);
} on DioException catch (e) {
  return Failure(AppException.dioError(e, fallback: 'Mensagem de fallback.'));
} catch (e, s) {
  return Failure(AppException.fatal('Erro inesperado.', stackTrace: s, originalError: e));
}
```

### 6. Strings visíveis ao usuário — sempre chaves de tradução

```dart
// ❌ Evitar
Text('Selecionar imagem')

// ✅ Correto
Text(context.tr('selectImageSource'))
```

Chaves ficam em `assets/translations/pt-BR.json`.

### 7. APIs depreciadas

| Depreciado             | Substituto                   |
| ---------------------- | ---------------------------- |
| `Color.withOpacity(x)` | `Color.withValues(alpha: x)` |
| `command.isRunning`    | `command.value.isRunning`    |

### 8. Design responsivo (mobile / tablet / desktop)

- Breakpoints: mobile `< 600`, tablet `600–1199`, desktop `>= 1200` (`lib/ui/core/responsive/`).
- Usar `context.screenSize`, `context.isMobile`, `context.isDesktop` — não `MediaQuery.size.width` cru para decidir layout.
- `ResponsiveLayout` quando a **árvore de widgets** muda entre breakpoints.
- `ResponsiveValue<T>` quando só **valores** mudam (padding, largura, altura).
- **Sempre** envolver conteúdo principal em `CenteredContent` com `maxWidth` adequado: feed `640`, formulários `440`, leitura/perfil `720`.
- Tokens obrigatórios em widgets compartilhados: `AppIconSizes`, `AppSpacing`, `AppRadii`, `AppTextScale` (`lib/ui/core/theme/responsive_tokens.dart`).
- **Proibido** `flutter_screenutil` ou escala global proporcional à largura da tela.
- Bottom nav, FAB e `Scaffold.drawer` **somente no mobile**; tablet/desktop usam rail lateral e menu “Mais”.

---

## Resumo por camada

### `config/` — Composition root

- Bootstrapping, DI (`auto_injector`), feature flags via `--dart-define-from-file`.
- **View models de sessão** (ex.: `AuthViewModel`): `addOther` → `rootProviders`.
- **View models de página**: `addViewModel` (factory) → nunca em `rootProviders`; provider declarado na rota em `routing/`.
- Sem regras de negócio, sem imports de `ui/`.

→ Detalhes: [lib/config/AGENTS.md](lib/config/AGENTS.md)

---

### `data/` — Infraestrutura

- Implementa contratos do `domain/`.
- Traduz DTOs ↔ entidades via mappers explícitos.
- Orquestra fontes (remoto, cache, local).
- Erros HTTP: `AppException.dioError` / `AppException.fatal`; sem `throw` em fluxos tratáveis.
- Sem regras de negócio; sem imports de `ui/`.

→ Detalhes: [lib/data/AGENTS.md](lib/data/AGENTS.md)

---

### `domain/` — Negócio

- Entidades, value objects, use cases, contratos (interfaces de repositório/serviço).
- Sem Flutter, sem `BuildContext`, sem infra direta.
- Use cases expõem `AsyncResult<T>`; validações convergem para `Failure`, não `throw`.
- Composição via `flatMap`/`map` com method tear-off sempre que possível.

→ Detalhes: [lib/domain/AGENTS.md](lib/domain/AGENTS.md)

---

### `routing/` — Navegação

- Paths centralizados (ex.: `RoutePaths`), sem strings mágicas.
- Guards (auth, onboarding) como redirects dedicados.
- Provider de página montado aqui via `ViewmodelProvider<T>`.
- Sem use cases, sem side effects de produto, sem widgets grandes inline.

→ Detalhes: [lib/routing/AGENTS.md](lib/routing/AGENTS.md)

---

### `ui/` — Apresentação

- Organizada por **feature** (page / widgets / viewmodels).
- **Sealed states**: cada subclasse = um estado exclusivo da UI. Proibido `bool isLoading` em god-state.
- **`switch` exaustivo** na UI para todos os casos da sealed class, sem `default` genérico.
- **Widgets privados** (`_MyWidget`) em vez de métodos `_buildXxx(context, ...)`.
- View model retorna `AsyncResult<T>`; página usa `fold` para side effects (toast, pop, navegação).
- `BuildContext` após `await`: sempre checar `if (!context.mounted) return;`.
- `initState` com efeitos colaterais: envolver em `WidgetsBinding.instance.addPostFrameCallback`.
- Modais de confirmação: preferir `showModalBottomSheet`; usar `showDialog` apenas para ações críticas irreversíveis.
- Páginas **nunca** acessam repositórios ou use cases diretamente — sempre pelo view model.

→ Detalhes: [lib/ui/AGENTS.md](lib/ui/AGENTS.md)

---

## Checklist antes de submeter uma mudança

- [ ] A camada correta foi modificada (sem vazamento de responsabilidade)?
- [ ] Retornos de métodos que podem falhar usam `Result` / `AsyncResult`?
- [ ] Nenhum `throw` em fluxo tratável — somente `Failure(...)`?
- [ ] Strings visíveis ao usuário usam `context.tr('chave')`?
- [ ] APIs depreciadas substituídas (`withOpacity` → `withValues`, etc.)?
- [ ] View model de página registrado com `addViewModel`, não `addOther`?
- [ ] `context.mounted` checado após `await` em widgets?
- [ ] Nenhum text literal hardcoded na UI?
- [ ] Página testada em mobile, tablet e desktop?
- [ ] Conteúdo principal em `CenteredContent` com `maxWidth` adequado?
- [ ] Ícones/espaçamentos/raios via tokens responsivos (não literais em widgets compartilhados)?
- [ ] Nenhum bottom nav / drawer em tablet/desktop?
