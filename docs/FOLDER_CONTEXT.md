# 📁 FOLDER_CONTEXT — docs/

## Instrução para LLM

Você está no diretório **docs/**. Esta pasta contém a **documentação técnica e operacional** do projeto BabaPlay, complementando os arquivos de documentação na raiz.

## Responsabilidade desta pasta

Armazenar guias e procedimentos que não cabem nos arquivos de raiz do repositório:

| Arquivo                      | Conteúdo                                                                          |
|------------------------------|-----------------------------------------------------------------------------------|
| `deploy-manual-docker.md`    | Guia passo-a-passo para deploy manual do BabaPlay em servidor VPS usando Docker   |

## Quando consultar esta pasta

- Antes de realizar um deploy manual em produção ou staging
- Para entender requisitos de infraestrutura do servidor
- Para solucionar problemas de containerização

## Regras para LLM ao atuar nesta pasta

1. **Documentação de processo** — esta pasta é para guias operacionais, não para código.
2. Ao documentar novos procedimentos (ex: deploy em cloud, backup de banco, configuração de SSL), criar um novo arquivo `.md` aqui.
3. Manter os guias atualizados quando houver mudanças no processo de deploy ou na infraestrutura.
4. Documentação de arquitetura e features fica nos arquivos da **raiz** (`architecture.md`, `functions.md`, etc.) — não duplicar aqui.
