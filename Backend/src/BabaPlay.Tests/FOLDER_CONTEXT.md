# 📁 FOLDER_CONTEXT — BabaPlay.Tests

## Instrução para LLM

Você está no projeto **BabaPlay.Tests**. Este é o projeto de **testes automatizados** do BabaPlay, cobrindo as camadas Domain, Application, Infrastructure e Web (Blazor).

## Responsabilidade deste projeto

Garantir a qualidade e a corretude do sistema através de testes automatizados em diferentes níveis:

| Diretório       | Tipo de Teste     | O que testa                                                                           |
|-----------------|-------------------|---------------------------------------------------------------------------------------|
| `Unit/`         | Testes unitários  | Lógica isolada das camadas Domain e Application (com mocks para dependências)         |
| `Unit/Domain/`  | Testes unitários  | Regras de negócio das Entities do Domain (sem dependências externas)                 |
| `Unit/Application/` | Testes unitários | Command/Query handlers com repositórios mockados                                 |
| `Unit/Infrastructure/` | Testes unitários | Serviços de infraestrutura com dependências mockadas                          |
| `Integration/`  | Testes de integração | Testes end-to-end contra banco de dados real (PostgreSQL in-memory ou de teste)  |
| `Web/`          | Testes de componente | Testes de componentes Blazor usando bUnit                                        |

## Stack de Testes

- **xUnit** — framework de testes principal
- **Moq** ou **NSubstitute** — mocking de dependências
- **FluentAssertions** — assertions legíveis
- **bUnit** — testes de componentes Blazor

## Regras para LLM ao atuar neste projeto

1. **Todo Command/Query handler** criado na Application deve ter um teste unitário correspondente em `Unit/Application/`.
2. **Toda Entity** com lógica de negócio deve ter testes unitários em `Unit/Domain/`.
3. **Mocks** devem ser criados para todos os repositórios e serviços externos nos testes unitários.
4. **Testes de integração** usam banco de dados de teste — nunca usar dados de produção.
5. **Nomenclatura** dos testes: `MetodoTestado_Cenario_ResultadoEsperado` (ex: `CreatePlayer_WhenPlayerAlreadyExists_ThrowsDomainException`).
6. **Testes Blazor** em `Web/` usam bUnit para renderizar componentes e verificar comportamento da UI.
7. Para executar os testes localmente, use o script `scripts/run-tests-local.sh` na raiz do projeto.
