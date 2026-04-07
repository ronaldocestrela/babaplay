# Processo de Release - BabaPlay Backend

Este documento descreve o fluxo operacional para publicacao de versoes.

## 1. Tipos de Release

- Release normal: inclui features e melhorias planejadas.
- Hotfix: correcao urgente em versao ja publicada.

## 2. Checklist de Preparacao

1. Confirmar escopo da release e itens em `CHANGELOG.md` (`Unreleased`).
2. Garantir branch limpa e sincronizada.
3. Executar build da solucao:

```bash
dotnet build BabaPlay.slnx
```

4. Se aplicavel, validar migrations EF pendentes e impacto de schema.

## 3. Atualizacao de Versao

Atualizar `Directory.Build.props` conforme a estrategia SemVer:

1. `VersionPrefix`
2. `VersionSuffix` (se houver pre-release)
3. `AssemblyVersion`
4. `FileVersion`
5. `InformationalVersion`

Regras praticas:

- Pre-release em andamento: avancar sufixo (`beta.1` -> `beta.2`).
- Release estavel: remover sufixo.
- Hotfix: incrementar `PATCH`.

## 4. Atualizacao do Changelog

1. Mover itens de `Unreleased` para nova secao versionada.
2. Registrar data no formato `YYYY-MM-DD`.
3. Conferir categorias (`Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`).
4. Ajustar links de comparacao/release no fim do arquivo.

## 5. Validacao Final Antes da Tag

Usar tambem o checklist de PR em `.github/PULL_REQUEST_TEMPLATE.md` para garantir consistencia de validacao.

1. Build final:

```bash
dotnet build BabaPlay.slnx
```

2. Revisar diff para garantir que apenas arquivos esperados entraram na release.
3. Confirmar que documentacao de setup e versionamento continuam consistentes.

## 6. Tag e Publicacao

1. Criar commit final da release.
2. Criar tag seguindo o padrao:

```bash
git tag vX.Y.Z
```

Ou pre-release:

```bash
git tag vX.Y.Z-beta.N
```

3. Publicar branch e tags:

```bash
git push origin <branch>
git push origin --tags
```

4. Criar release notes no provedor Git com base no `CHANGELOG.md`.

## 7. Fluxo de Hotfix

1. Criar branch de hotfix a partir da versao publicada.
2. Aplicar apenas correcao necessaria.
3. Incrementar `PATCH`.
4. Atualizar `CHANGELOG.md` com secao de hotfix.
5. Repetir validacao, tag e publicacao.

## 8. Rollback e Pos-release

- Em caso de incidente, registar decisao de rollback e causa raiz.
- Criar acao corretiva para evitar recorrencia.
- Atualizar changelog/documentacao quando houver ajuste apos release.

## 9. Referencias

- `docs/VERSIONING.md`
- `CHANGELOG.md`
- `Directory.Build.props`
- `.github/PULL_REQUEST_TEMPLATE.md`