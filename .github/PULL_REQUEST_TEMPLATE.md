# Resumo

Descreva em poucas linhas o objetivo deste PR.

## Tipo de mudanca

- [ ] Feature
- [ ] Fix
- [ ] Refactor
- [ ] Hotfix
- [ ] Release
- [ ] Documentacao

## Checklist geral

- [ ] Escopo e motivacao estao claros no resumo.
- [ ] Mudancas seguem os padroes da arquitetura do projeto.
- [ ] Build local executado com sucesso (`dotnet build BabaPlay.slnx`).
- [ ] Mudancas relevantes estao documentadas em `CHANGELOG.md` (secao `Unreleased` ou secao de release).
- [ ] Nao foram adicionados segredos em codigo, docs ou configuracoes versionadas.

## Checklist de release/hotfix (preencher quando aplicavel)

- [ ] `Directory.Build.props` atualizado com a versao alvo (`VersionPrefix`, `VersionSuffix`, `AssemblyVersion`, `FileVersion`, `InformationalVersion`).
- [ ] `CHANGELOG.md` atualizado com data e secoes corretas (`Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`).
- [ ] Confirmado impacto de migrations EF (quando houver alteracao de schema).
- [ ] Validacao final executada (`dotnet build BabaPlay.slnx`).
- [ ] Tag planejada segue o padrao (`vX.Y.Z` ou `vX.Y.Z-beta.N`).
- [ ] Release notes serao publicadas com base no `CHANGELOG.md`.

## Evidencias

Inclua evidencias objetivas (saida de build, prints de comportamento, payloads de teste, etc.).

## Observacoes

Riscos conhecidos, rollback e pontos de atencao para revisao/deploy.