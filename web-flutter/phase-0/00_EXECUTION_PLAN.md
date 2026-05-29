# Fase 0 - Execution Plan (BigBang)

## Objetivo
Executar preparacao e congelamento da migracao React -> Flutter BigBang, sem iniciar implementacao Flutter, com foco em previsibilidade e paridade.

## Entradas obrigatorias
- [../AGENTS.md](../AGENTS.md)
- [../ROADMAP_REFACTORACAO_FLUTTER.md](../ROADMAP_REFACTORACAO_FLUTTER.md)
- [../../web/src/router.tsx](../../web/src/router.tsx)
- [../../web/src/core/api/client.ts](../../web/src/core/api/client.ts)
- [../../web/src/core/constants/apiRoutes.ts](../../web/src/core/constants/apiRoutes.ts)
- [../../web/src/index.css](../../web/src/index.css)
- [../../web/src/test/handlers.ts](../../web/src/test/handlers.ts)
- [../../web/vite.config.ts](../../web/vite.config.ts)

## Timebox
- Duracao alvo: 3 a 5 dias uteis.

## Plano diario
1. D1: publicar cutoff e congelamento do React.
2. D1-D2: fechar inventario de rotas/guards e matriz de paridade.
3. D2-D3: capturar baseline visual por breakpoint e por estado.
4. D3-D4: fechar cenarios criticos de auth/tenant/checkin.
5. D4-D5: consolidar riscos, mitigacoes e pacote de aceite da fase.

## Donos sugeridos
- Tech lead: decisao de corte e aceite final.
- Frontend legado: inventario de fluxos e baseline visual.
- Backend: validacao de contratos e codigos de erro.
- QA: checklist de paridade e evidencias.

## Gate de saida da Fase 0
- [ ] Cutoff ativo e comunicado.
- [ ] Matriz de paridade preenchida e validada.
- [ ] Baseline visual completo (mobile/tablet/desktop).
- [ ] Cenarios criticos aprovados com criterios de aceite.
- [ ] Registro de riscos com mitigacao e responsavel.
- [ ] Backlog da Fase 1 priorizado por risco.
