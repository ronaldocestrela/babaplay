# Fase 0 - Cutoff Policy (React Freeze)

## Objetivo
Evitar drift entre o frontend React e a migracao Flutter durante o modo BigBang.

## Regra principal
A partir da data de cutoff, nenhuma feature nova entra no frontend React.

## Modelo de governanca
1. Permitido no React apos cutoff:
- Bug critico P0/P1.
- Fix de seguranca.
- Ajuste legal/compliance com prazo externo.

2. Nao permitido no React apos cutoff:
- Novas features.
- Refactor nao critico.
- Mudanca visual sem impacto de incidente.

3. Fluxo de aprovacao de excecao:
- Abrir issue com tag `react-freeze-exception`.
- Aprovar por tech lead + product owner.
- Registrar impacto na matriz de paridade.

## Branching sugerido
- Branch legado congelado: `react-frozen`.
- Branch migracao: `refact/refact-react-to-flutter`.
- Merge para legado apenas por excecao aprovada.

## Checklist operacional
- [ ] Data e hora de cutoff definidas.
- [ ] Time comunicado (dev/qa/produto).
- [ ] Template de excecao criado.
- [ ] Quadro de monitoramento de excecoes ativo.

## Evidencias de compliance
- Print da comunicacao de cutoff.
- Lista de excecoes aprovadas.
- Changelog de excecoes com motivo e owner.
