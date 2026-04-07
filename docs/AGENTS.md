# AGENTS — docs

## Conteúdo

- Especificação-alvo para o MVP SaaS de associações esportivas.

## Para assistentes

- Use [llm_implementation_spec_saa_s_associacoes_esportivas.md](llm_implementation_spec_saa_s_associacoes_esportivas.md) como **requisitos de produto**, não como reflexo automático do código.
- Se a spec e o código diferirem, confirme com o utilizador ou alinhe a implementação; não altere este ficheiro por iniciativa própria salvo pedido explícito.

## Documentação frontend (API consumida pelo cliente)

- Pasta [frontend/](frontend/) — um ficheiro por **controller** com payloads e respostas esperadas (`ApiResponse`).
- Configuração de equipas: [associations-controller.md](frontend/associations-controller.md) (`playersPerTeam`) e [teams-controller.md](frontend/teams-controller.md) (geração só com `sessionId`).

## Evitar

- Não colocar código-fonte em `docs/` sem pedido explícito.
