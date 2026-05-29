# Fase 0 - Risk Register

## Uso
Atualizar status e dono diariamente durante a Fase 0.

Legenda:
- Probabilidade: Baixa / Media / Alta
- Impacto: Baixo / Medio / Alto
- Status: Open / Mitigating / Closed

| ID | Risco | Probabilidade | Impacto | Mitigacao | Owner | Status |
|---|---|---|---|---|---|---|
| R01 | Violacao de arquitetura em camadas no Flutter | Alta | Alto | Validar regras AGENTS e revisar vertical slice antes de escalar | Tech Lead | Open |
| R02 | Divergencia visual entre React e Flutter | Alta | Medio | Baseline por breakpoint + golden tests | FE Lead | Open |
| R03 | Regressao em auth refresh concorrente | Media | Alto | Prototipo de interceptor com fila + testes de concorrencia | FE Lead | Open |
| R04 | Guardas de tenant/onboarding inconsistentes | Media | Alto | Matriz de rotas e cenarios criticos assinados | FE + QA | Open |
| R05 | Fluxo de geo/mapa com falha em permissao | Media | Medio | Definir fallback manual e caso de erro padrao | FE | Open |
| R06 | BigBang sem rollback praticavel | Media | Alto | Ensaiar plano de rollback antes do cutover | Tech Lead + DevOps | Open |
| R07 | Drift do React apos cutoff | Alta | Alto | Freeze policy com excecao formal e rastreada | Product + Tech Lead | Open |

## Gate de saida da Fase 0
- [ ] Todos os riscos criticos com mitigacao ativa.
- [ ] Nenhum risco de arquitetura sem owner.
- [ ] Riscos do cutover com plano de rollback definido.
