# Roadmap de Refatoracao React -> Flutter (BigBang)

## Objetivo
Migrar o frontend web atual para Flutter em modo BigBang, mantendo a identidade visual atual, paridade funcional e regras arquiteturais definidas em [AGENTS.md](AGENTS.md).

## Escopo BigBang
- Substituir integralmente o frontend React por Flutter no go-live.
- Manter padrao visual (cores, tipografia, espacamento, componentes e comportamento).
- Preservar fluxos criticos: auth, tenant context, onboarding, check-in, dashboard e settings.
- Garantir paridade de regras de negocio expostas pela API atual.

## Principios obrigatorios (derivados do AGENTS)
- Dependencia unidirecional: ui -> domain <- data.
- Domain sem Flutter e sem dependencia de data/ui.
- Metodos falhiveis retornam Result/AsyncResult.
- Sem throw para fluxo controlado (usar Failure).
- UI com estados selados e switch exaustivo.
- Strings visiveis via traducao (context.tr).
- Responsividade obrigatoria (mobile/tablet/desktop) com tokens e componentes base.

## Macro cronograma (estimativa)
- Fase 0 a Fase 2: 3 a 4 semanas
- Fase 3 e Fase 4: 6 a 9 semanas
- Fase 5 a Fase 7: 3 a 5 semanas
- Total estimado: 12 a 18 semanas

## Fase 0 - Preparacao e congelamento
Duracao: 3 a 5 dias

### Pacote de execucao
- [phase-0/00_EXECUTION_PLAN.md](phase-0/00_EXECUTION_PLAN.md)
- [phase-0/01_CUTOFF_POLICY.md](phase-0/01_CUTOFF_POLICY.md)
- [phase-0/02_ROUTE_PARITY_MATRIX.md](phase-0/02_ROUTE_PARITY_MATRIX.md)
- [phase-0/03_VISUAL_BASELINE_CHECKLIST.md](phase-0/03_VISUAL_BASELINE_CHECKLIST.md)
- [phase-0/04_CRITICAL_SCENARIOS.md](phase-0/04_CRITICAL_SCENARIOS.md)
- [phase-0/05_TEST_ASSETS_PARITY.md](phase-0/05_TEST_ASSETS_PARITY.md)
- [phase-0/06_RISK_REGISTER.md](phase-0/06_RISK_REGISTER.md)

### Entregaveis
- Congelamento de novas features no frontend React.
- Inventario de telas, fluxos e estados atuais.
- Baseline visual oficial (capturas por tela e breakpoint).
- Matriz de paridade funcional por rota.

### Criterios de aceite
- Todas as rotas mapeadas com regras de acesso e transicao.
- Baseline visual aprovado por produto/design.
- Backlog de migracao priorizado por risco.

## Fase 1 - Foundation Flutter
Duracao: 1 a 2 semanas

### Entregaveis
- Estrutura inicial por camadas:
  - lib/config
  - lib/data
  - lib/domain
  - lib/routing
  - lib/ui
- Config de ambientes e DI (composition root).
- Infra base de erros, Result/AsyncResult e AppException.
- Setup de l10n, tema base, responsividade e tokens.
- Pipeline de analise estatica, formatacao e testes.

### Criterios de aceite
- Build local em mobile/tablet/desktop sem warnings criticos.
- Regras de dependencia entre camadas validadas.
- Exemplo vertical slice simples aprovado (sem bypass de arquitetura).

## Fase 2 - Paridade visual (Design System)
Duracao: 1 a 2 semanas (paralelo parcial com Fase 1)

### Entregaveis
- Tema Flutter refletindo identidade atual:
  - Paleta
  - Tipografia
  - Elevacao e bordas
  - Espacamento e raios
- Biblioteca de widgets base:
  - Botao, input, select, card, modal, snackbar, loading, empty state
- Guia de uso com exemplos por breakpoint.

### Criterios de aceite
- Aprovacao visual por checklist de paridade.
- Golden tests de componentes base.
- Nenhum componente compartilhado com valores literais de espacamento/icone/raio fora dos tokens.

## Fase 3 - Shell da aplicacao e fluxos transversais
Duracao: 2 a 3 semanas

### Entregaveis
- Routing com guards:
  - autenticacao
  - onboarding
  - selecao de tenant
- Sessao e auth:
  - login
  - refresh token com fila
  - logout
  - perfil atual
- Tenant context:
  - header X-Tenant-Slug
  - troca e persistencia de tenant
- Tratamento padrao de erro por codigo da API.

### Criterios de aceite
- Paridade de comportamento com web atual em auth/guard.
- Fluxo de refresh resiliente com requests concorrentes.
- Cobertura de testes para auth e interceptors.

## Fase 4 - Migracao de features por dominio
Duracao: 4 a 6 semanas

### Ordem recomendada (baixo para alto risco)
1. Dashboard
2. Players
3. Teams
4. Matches
5. Positions
6. Tenant Settings
7. Tenant Onboarding e Invites
8. Check-ins com geolocalizacao e mapa

### Entregaveis por feature
- Domain:
  - entidades
  - contratos
  - use cases (AsyncResult)
- Data:
  - datasource/client
  - DTOs e mappers
  - repositorio
- UI:
  - page
  - view model
  - widgets
  - estados selados
- Testes:
  - unitarios de use case e view model
  - widget tests da feature

### Criterios de aceite por feature
- Paridade funcional (regras e mensagens de erro).
- Paridade visual por breakpoint.
- Sem acesso direto de page para repositorio/use case.
- Sem texto hardcoded em UI.

## Fase 5 - Qualidade, performance e seguranca
Duracao: 1 a 2 semanas

### Entregaveis
- Suite de regressao funcional completa.
- Golden tests das telas criticas.
- Testes de navegacao e guards.
- Hardening de erros de rede e timeout.
- Revisao de segredos e configuracoes sensiveis.

### Criterios de aceite
- Taxa de sucesso alta em regressao automatizada.
- Sem blockers de seguranca para release.
- Metricas minimas de performance definidas e atingidas.

## Fase 6 - Cutover BigBang
Duracao: 2 a 4 dias

### Entregaveis
- Plano de release com janela de corte.
- Plano de rollback documentado e testado.
- Checklist operacional de go-live.
- Monitoramento e logs do primeiro dia.

### Criterios de aceite
- Go-live sem perda de funcionalidade critica.
- Rollback possivel em janela acordada.
- Incidentes P0/P1 sob controle.

## Fase 7 - Estabilizacao pos-go-live
Duracao: 1 a 2 semanas

### Entregaveis
- Correcao de bugs de producao por prioridade.
- Ajustes finos de UX/performance.
- Encerramento do frontend antigo (decomissionamento controlado).

### Criterios de aceite
- Volume de incidentes estabilizado.
- SLOs operacionais dentro do esperado.
- Frontend legado desativado com seguranca.

## Definition of Done (global)
- Arquitetura respeitada conforme [AGENTS.md](AGENTS.md).
- Paridade funcional validada por roteiro de testes.
- Paridade visual validada por baseline e goldens.
- Cobertura de testes acordada por modulo.
- Sem segredos hardcoded no app.
- Documentacao tecnica e runbook atualizados.

## Matriz de riscos
| Risco | Impacto | Mitigacao |
|---|---|---|
| Divergencia visual em telas grandes | Medio/Alto | Baseline + golden tests + revisao por breakpoint |
| Regressao em auth/refresh | Alto | Testes de concorrencia + testes de integracao de interceptors |
| Complexidade de mapa/geolocalizacao | Alto | Prototipo antecipado e fallback de permissao/localizacao |
| Regras de tenant inconsistentes | Alto | Testes de guard + testes E2E de troca de tenant |
| BigBang sem rollback robusto | Alto | Ensaiar rollback e checklist operacional antes do corte |

## Plano de execucao semanal (resumo)
- Semana 1: Fase 0 + inicio Fase 1
- Semana 2: Fase 1 + Fase 2
- Semana 3: Fase 3
- Semana 4 a 7: Fase 4 (features por ordem de risco)
- Semana 8: Fase 5
- Semana 9: Fase 6
- Semana 10: Fase 7

## Governanca sugerida
- Daily tecnico de migracao (15 min).
- Demo semanal de paridade por modulo.
- Gate de qualidade por fase (nao avanca sem criterios de aceite).
- Dono tecnico por camada (config/data/domain/routing/ui).

## Observacoes finais
- BigBang funciona melhor com escopo congelado e disciplina de paridade.
- Priorize previsibilidade: menos improviso, mais checklist.
- Qualidade de base (Fase 1 e Fase 2) reduz custo de retrabalho nas fases de feature.
