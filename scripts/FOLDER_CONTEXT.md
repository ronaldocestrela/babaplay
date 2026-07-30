# 📁 FOLDER_CONTEXT — scripts/

## Instrução para LLM

Você está no diretório **scripts/**. Esta pasta contém scripts utilitários para automatizar tarefas de desenvolvimento, CI e testes locais.

## Responsabilidade desta pasta

Fornecer automações reutilizáveis para o time de desenvolvimento:

| Script                  | Responsabilidade                                                                        |
|-------------------------|-----------------------------------------------------------------------------------------|
| `ci-local.sh`           | Simula o pipeline de CI localmente: build, lint, testes — útil para validar antes de push |
| `run-tests-local.sh`    | Executa a suite de testes automatizados (`BabaPlay.Tests`) no ambiente local           |

## Quando usar cada script

- **`ci-local.sh`** — Antes de abrir um Pull Request para garantir que tudo passa no CI.
- **`run-tests-local.sh`** — Durante o desenvolvimento para rodar apenas os testes rapidamente.

## Regras para LLM ao atuar nesta pasta

1. **Dar permissão de execução** aos scripts antes de rodar: `chmod +x scripts/ci-local.sh`.
2. Scripts devem ser executados a partir da **raiz do projeto** (diretório `/home/rony/LPR/babaplay/`).
3. Ao criar novos scripts, seguir o padrão de nomenclatura `kebab-case.sh` e adicionar comentários de cabeçalho explicando o propósito.
4. Scripts de CI nunca devem modificar dados de produção ou fazer push automático.
