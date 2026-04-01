# Versionamento - BabaPlay Backend

Este documento define a politica de versionamento do BabaPlay Backend.

## 1. Objetivo

- Manter previsibilidade de mudancas para equipa tecnica e consumidores da API.
- Padronizar como a versao e evoluida no codigo, changelog e tags Git.
- Reduzir risco em releases, hotfixes e breaking changes.

## 2. Fonte de Verdade da Versao

Os campos centrais de versao vivem em `Directory.Build.props`:

- `VersionPrefix`
- `VersionSuffix`
- `AssemblyVersion`
- `FileVersion`
- `InformationalVersion`

No estado atual do projeto:

- `VersionPrefix`: `0.1.0`
- `VersionSuffix`: `beta.1`
- `InformationalVersion`: `0.1.0-beta.1`

## 3. Convencao SemVer

O projeto adota Semantic Versioning (`MAJOR.MINOR.PATCH`).

- `MAJOR`: mudancas incompativeis para consumidores.
- `MINOR`: funcionalidades novas compativeis.
- `PATCH`: correcoes e ajustes compativeis.

## 4. Politica Beta (0.x)

Enquanto a versao principal for `0.x`, o produto esta em pre-release:

- Breaking changes podem acontecer antes de `1.0.0`.
- Mesmo assim, toda quebra deve ser documentada no `CHANGELOG.md`.
- Sempre que possivel, preferir transicao suave (deprecacao) antes de remover comportamento.

## 5. Sufixos de Pre-release

Sufixos suportados:

- `-alpha.N`
- `-beta.N`
- `-rc.N`

Exemplos:

- `0.2.0-alpha.1`
- `0.2.0-beta.3`
- `1.0.0-rc.1`

## 6. Regras de Bump

- Nova feature compativel: incrementar `MINOR`.
- Correcao sem quebra: incrementar `PATCH`.
- Quebra de contrato: incrementar `MAJOR` (ou documentar explicitamente se ainda em `0.x`).

Exemplos praticos:

- `0.1.0-beta.1` -> `0.1.0-beta.2` (evolucao do mesmo ciclo beta)
- `0.1.0` -> `0.2.0` (novas features compativeis)
- `0.2.3` -> `0.2.4` (hotfix)
- `0.9.0` -> `1.0.0` (marco de estabilidade)

## 7. Changelog

O `CHANGELOG.md` segue Keep a Changelog e deve permanecer atualizado:

- Trabalhos em andamento na secao `Unreleased`.
- Na release, mover itens de `Unreleased` para a secao da nova versao com data.
- Classificar mudancas em secoes padrao (`Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`).

## 8. Tags Git e Releases

Padrao de tag:

- `vMAJOR.MINOR.PATCH`
- `vMAJOR.MINOR.PATCH-suffix` (pre-release)

Exemplos:

- `v0.1.0-beta.1`
- `v0.1.0`

Cada tag de release deve ter release notes correspondentes.

## 9. Banco de Dados e Migrations

Mudancas de schema devem seguir estas regras:

- Toda alteracao de modelo deve ter migration correspondente.
- Nome da migration deve refletir claramente a intencao.
- Breaking changes de schema devem ser destacadas no `CHANGELOG.md`.

## 10. Containers e Artefatos

- Evitar promover artefatos apenas com `latest` em processos de release.
- Sempre que possivel, publicar imagem com tag da versao da release.
- O uso de `latest` pode permanecer para ambiente de desenvolvimento/controlado.

## 11. Compatibilidade e Deprecacao

- Priorizar compatibilidade para endpoints e payloads existentes.
- Quando houver deprecacao, documentar no changelog e indicar alternativa.
- Remocoes devem ser comunicadas com antecedencia razoavel dentro do ciclo do produto.

## 12. Referencias

- `CHANGELOG.md`
- `Directory.Build.props`
- `docs/RELEASE_PROCESS.md`