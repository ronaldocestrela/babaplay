# Guia de Implementação e Migração Blazor (.NET 10) - Baba Play

Este documento serve como o **guia definitivo e sequencial** para a criação do Front-end em **Blazor (.NET 10)** do **Baba Play**, substituindo o antigo front-end em React.

Todas as diretrizes e regras definidas em [`agents.md`](file:///home/rony/LPR/babaplay/agents.md) e [`architecture.md`](file:///home/rony/LPR/babaplay/architecture.md) devem ser rigorosamente seguidas durante a implementação de cada função.

---

## 1. Regras Arquiteturais e Padrões Blazor (.NET 10)

1. **Separação Estrita e Desacoplamento:**
   * O projeto Blazor (`BabaPlay.Web`) é **100% cliente da API**. 
   * Não há referência direta ao Entity Framework Core, DbContext ou pacotes de persistência do backend.
   * Toda a comunicação ocorre exclusivamente via requisições HTTP REST chamando os endpoints CQRS da Web API (`BabaPlay.Api`).
   * Os dados trafegam usando DTOs (Data Transfer Objects) dedicados ao front-end.

2. **Multitenancy & Segurança:**
   * O token JWT obtido no login armazena as claims do usuário e as associações (tenants) das quais ele faz parte.
   * Cada requisição HTTP enviada pelos services do Blazor deve incluir o cabeçalho `Authorization: Bearer <token>` e `X-Tenant-Id: <tenant-guid>` (ou via claim do token).
   * As rotas protegidas utilizam `AuthorizeRouteView` e `CascadingAuthenticationState`.

3. **Gerenciamento de Estado (State Management):**
   * Usar serviços com escopo (`Scoped`) para manter o estado da sessão do usuário (`UserSessionState`), tenant ativo (`TenantState`) e carrinho/ações temporárias.
   * Notificação de alterações de estado via `event Action OnChange`.

4. **Metodologia TDD (Test-Driven Development no Blazor):**
   * A criação de componentes e services deve ser acompanhada de testes unitários e de renderização usando **bUnit** e **xUnit**.
   * Testar estados de carregamento (Loading), erro (Error banner), sucesso e formulários com validação (`EditForm`, `DataAnnotationsValidator`).

---

## 2. Estrutura Recomendada do Projeto Blazor

```text
Backend/src/BabaPlay.Web/ (ou src/BabaPlay.Blazor/)
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── NavMenu.razor
│   │   ├── Header.razor
│   │   └── PublicLayout.razor
│   ├── Common/
│   │   ├── LoadingSpinner.razor
│   │   ├── ConfirmModal.razor
│   │   ├── Badge.razor
│   │   └── StatCard.razor
├── Features/
│   ├── Auth/
│   ├── Onboarding/
│   ├── Players/
│   ├── Matches/
│   ├── Teams/
│   ├── Financial/
│   └── Communication/
├── Services/
│   ├── Http/
│   │   ├── AuthApiService.cs
│   │   ├── PlayerApiService.cs
│   │   ├── MatchApiService.cs
│   │   └── ...
│   ├── State/
│   │   ├── CustomAuthStateProvider.cs
│   │   ├── TenantState.cs
│   │   └── UserSessionState.cs
│   └── Handlers/
│       └── AuthorizationHeaderHandler.cs
├── Models/ DTOs do Frontend
└── App.razor
```

---

## 3. Ordem Lógica de Construção (Fases & Dependências)

A construção deve obrigatoriamente seguir a ordem de dependência lógica de negócio:

```mermaid
flowchart TD
    F0[Fase 0: Infraestrutura Base & State Providers] --> F1[Fase 1: Autenticação, Onboarding & Multitenancy]
    F1 --> F2[Fase 2: Gestão de Atletas & Posições]
    F2 --> F3[Fase 3: Logística Esportiva & Sorteio de Times]
    F3 --> F4[Fase 4: Módulo Financeiro & Mensalidades]
    F3 --> F5[Fase 5: Comunicação & Real-time SignalR]
    F1 --> F6[Fase 6: Configurações do Tenant]
```

---

## 4. Guia Detalhado de Implementação por Função

---

### Fase 0: Infraestrutura Base do Blazor

#### **[F00] Setup da Aplicação, Layout Master & HTTP Client Base**
* **Objetivo:** Criar o projeto Blazor WebAssembly / Auto em .NET 10, configurar a injeção de dependência do `HttpClient`, manipulador de Token JWT (`AuthorizationHeaderHandler`) e o sistema de design (CSS/Layout).
* **Componentes / Arquivos:**
  * `Program.cs` (Configuração de DI, `HttpClient`, `AuthenticationStateProvider`)
  * `Services/Handlers/AuthorizationHeaderHandler.cs` (Injeta Bearer Token e `X-Tenant-Id`)
  * `Services/State/CustomAuthStateProvider.cs` (Gerencia estado do JWT e Claims)
  * `Components/Layout/MainLayout.razor` & `PublicLayout.razor`
* **Testes bUnit/xUnit:** Testar se o `AuthorizationHeaderHandler` anexa os cabeçalhos corretos e se `CustomAuthStateProvider` parseia as claims do JWT corretamente.

---

### Fase 1: Módulo Identity (Autenticação, Onboarding & Multitenancy)

#### **[F01] Autenticação (Login & Logout)**
* **Objetivo:** Permitir login via e-mail e senha, armazenar o token JWT com segurança (localStorage/sessionStorage via IJSRuntime/ProtectedLocalStorage) e atualizar o estado global do usuário.
* **Componentes / Páginas:** `Pages/Auth/Login.razor`
* **Services & DTOs:**
  * `IAuthApiService` (`LoginAsync(LoginDto dto)`)
  * `LoginDto`, `AuthResponseDto`
* **Endpoints API Consumidos:** `POST /api/v1/auth/login`
* **Testes bUnit:**
  * Renderização de campos e validação de e-mail/senha.
  * Comportamento ao receber 401 Unauthorized (mensagem de erro).
  * Redirecionamento correto pós-login para a dashboard ou seleção de tenant.

#### **[F02] Recuperação e Redefinição de Senha**
* **Objetivo:** Solicitar link de redefinição por e-mail e processar a troca de senha com o token de recuperação.
* **Componentes / Páginas:** 
  * `Pages/Auth/ForgotPassword.razor`
  * `Pages/Auth/ResetPassword.razor`
* **Services & DTOs:**
  * `IAuthApiService` (`ForgotPasswordAsync`, `ResetPasswordAsync`)
  * `ForgotPasswordDto`, `ResetPasswordDto`
* **Endpoints API Consumidos:** `POST /api/v1/auth/forgot-password`, `POST /api/v1/auth/reset-password`
* **Testes bUnit:** Validação de formato de senha, correspondência entre "Nova Senha" e "Confirmação", feedback visual de sucesso.

#### **[F03] Seleção de Associação (Tenant Selector)**
* **Objetivo:** Se o usuário pertencer a múltiplos Tenants, permitir a escolha de qual associação deseja acessar na sessão atual.
* **Componentes / Páginas:** `Pages/Tenant/SelectTenant.razor`
* **Services & DTOs:** `TenantState.cs`, `TenantSummaryDto`
* **Endpoints API Consumidos:** `GET /api/v1/tenants/my-memberships`
* **Testes bUnit:** Testar a renderização dos cards das associações e a alteração do `TenantState.CurrentTenantId` no clique.

#### **[F04] Onboarding de Associação (Criar Novo Tenant)**
* **Objetivo:** Formulário para cadastrar uma nova associação/liga (White Label, upload de logotipo, cores primárias/secundárias e dados da diretoria).
* **Componentes / Páginas:** `Pages/Tenant/RegisterAssociation.razor`
* **Services & DTOs:** `ITenantApiService` (`CreateTenantAsync`), `CreateTenantDto`
* **Endpoints API Consumidos:** `POST /api/v1/tenants` (`CreateTenantCommand`)
* **Testes bUnit:** Validação do formulário de criação e envio de dados multipart/json para upload do escudo.

#### **[F05] Aceitar Convite de Associação**
* **Objetivo:** Tela pública onde um atleta/membro clica no link recebido (com token) para vincular sua conta a um Tenant existente.
* **Componentes / Páginas:** `Pages/Tenant/AcceptInvite.razor`
* **Services & DTOs:** `ITenantApiService` (`AcceptInviteAsync`), `AcceptInviteDto`
* **Endpoints API Consumidos:** `POST /api/v1/tenants/invites/accept`
* **Testes bUnit:** Testar validação de token expirado ou inválido e feedback de vínculo efetuado.

#### **[F06] Conclusão de Perfil do Atleta (Player Onboarding)**
* **Objetivo:** Primeiro acesso do atleta para preenchimento obrigatorio de pé preferido, posição principal, número da camisa e foto.
* **Componentes / Páginas:** `Pages/Players/CompleteProfile.razor`
* **Services & DTOs:** `IPlayerApiService` (`CompleteProfileAsync`), `PlayerProfileDto`
* **Endpoints API Consumidos:** `PUT /api/v1/players/profile`
* **Testes bUnit:** Garantir que o formulário impede o avanço enquanto campos obrigatórios (ex: posição) não forem preenchidos.

---

### Fase 2: Gestão de Atletas e Posições (Identity / Sports Base) ✅ CONCLUÍDA

#### **[F07] Lista e Gestão de Atletas (Membros da Associação)** ✅ CONCLUÍDO
* **Objetivo:** Tabela/Grid responsiva de atletas do Tenant, permitindo busca por nome, filtro por posição/status (Ativo/Inativo), alteração de perfil/função (RBAC: Admin, Treinador, Atleta).
* **Componentes / Páginas:** 
  * `Pages/Players/PlayerList.razor`
  * `Components/Players/PlayerCard.razor`
  * `Components/Players/EditPlayerModal.razor`
* **Services & DTOs:** `IPlayerApiService`, `PlayerDto`, `UpdatePlayerAdminDto`, `RoleDto`
* **Endpoints API Consumidos:** `GET /api/v1/player`, `POST /api/v1/role/{roleId}/users/{userId}`, `PUT /api/v1/player/{id}`
* **Testes bUnit:** `PlayerCardTests.cs`, `EditPlayerModalTests.cs`, `PlayerListTests.cs`.

#### **[F08] Cadastro e Gestão de Posições e Categorias** ✅ CONCLUÍDO
* **Objetivo:** Cadastro de posições personalizadas do baba (ex: Goleiro, Fixo, Ala, Pivô, Zagueiro, Meia, Atacante) e níveis de habilidade.
* **Componentes / Páginas:** 
  * `Pages/Settings/PositionsManagement.razor`
  * `Components/Settings/PositionModal.razor`
* **Services & DTOs:** `IPositionApiService`, `PositionDto`, `CreatePositionDto`, `UpdatePositionDto`
* **Endpoints API Consumidos:** `GET /api/v1/position`, `POST /api/v1/position`, `PUT /api/v1/position/{id}`, `DELETE /api/v1/position/{id}`
* **Testes bUnit:** `PositionModalTests.cs`, `PositionsManagementTests.cs`.

#### **[F09] Carteirinha Digital do Associado** ✅ CONCLUÍDO
* **Objetivo:** Componente visual para renderizar a carteirinha virtual do associado, contendo QR Code para validação de acesso, foto, validade e status.
* **Componentes / Páginas:** 
  * `Pages/Players/DigitalIdCard.razor`
  * `Components/Players/DigitalIdCardWidget.razor`
  * `Services/Helpers/QrCodeSvgHelper.cs`
* **Services & DTOs:** `IPlayerApiService` (`GetDigitalIdCardAsync`), `DigitalCardDto`
* **Endpoints API Consumidos:** `GET /api/v1/player/digital-card`
* **Testes bUnit:** `DigitalIdCardWidgetTests.cs`, `DigitalIdCardPageTests.cs`.


---

### Fase 3: Logística Esportiva, Sorteio e Partidas (`BabaPlay.Sports`)

#### **[F10] Dashboard Principal do Tenant**
* **Objetivo:** Painel de controle da associação exibindo métricas rápidas (próximo jogo, contagem de confirmados, últimos comunicados, estatísticas da temporada).
* **Componentes / Páginas:** 
  * `Pages/Dashboard/Dashboard.razor`
  * `Components/Dashboard/NextMatchWidget.razor`
  * `Components/Dashboard/QuickStatsWidget.razor`
* **Services & DTOs:** `IDashboardApiService`, `DashboardSummaryDto`
* **Endpoints API Consumidos:** `GET /api/v1/dashboard/summary`
* **Testes bUnit:** Renderização dos widgets de acordo com a resposta do DTO e tratamento de estados vazios (ex: sem jogos agendados).

#### **[F11] Agendamento e Calendário de Partidas/Treinos**
* **Objetivo:** Criar, editar e visualizar o calendário de eventos esportivos (local, data/hora, limite de vagas, valor por atleta se houver).
* **Componentes / Páginas:** 
  * `Pages/Matches/MatchList.razor`
  * `Pages/Matches/CreateMatch.razor`
  * `Components/Matches/MatchCard.razor`
* **Services & DTOs:** `IMatchApiService`, `ScheduleMatchDto`, `MatchSummaryDto`
* **Endpoints API Consumidos:** `GET /api/v1/matches`, `POST /api/v1/matches` (`ScheduleMatchCommand`)
* **Testes bUnit:** Validação de datas passadas no formulário de agendamento e exibição dos detalhes da partida.

#### **[F12] Sistema de RSVP / Check-in de Presença**
* **Objetivo:** Atletas confirmam ou recusam presença no próximo baba com um clique. Controle de lista de espera quando o limite de vagas for atingido.
* **Componentes / Páginas:** `Pages/Matches/MatchCheckin.razor`
* **Services & DTOs:** `IMatchApiService` (`SubmitRsvpAsync`), `RsvpSubmissionDto`, `MatchRsvpListDto`
* **Endpoints API Consumidos:** `POST /api/v1/matches/{id}/rsvp` (`SubmitRsvpCommand`), `GET /api/v1/matches/{id}/rsvp`
* **Testes bUnit:**
  * Alteração do botão de "Vou" para "Não Vou" atualizando o estado do componente instantaneamente.
  * Exibição do indicador de "Lista de Espera" quando as vagas estourarem.

#### **[F13] Algoritmo e Tela de Sorteio de Times (Coletes)**
* **Objetivo:** Interface interativa para acionar o algoritmo de balanceamento de times (com base no rating dos jogadores confirmados) e permitir ajustes manuais (drag-and-drop ou botões de troca).
* **Componentes / Páginas:** 
  * `Pages/Teams/TeamDraw.razor`
  * `Components/Teams/TeamColumn.razor`
  * `Components/Teams/PlayerBadge.razor`
* **Services & DTOs:** `ITeamApiService`, `GenerateBalancedTeamsDto`, `DrawResultDto`
* **Endpoints API Consumidos:** `POST /api/v1/matches/{id}/draw-teams` (`GenerateBalancedTeamsCommand`)
* **Testes bUnit:**
  * Testar o disparo do comando de geração de times.
  * Testar a movimentação de um jogador do Time Amarelo para o Time Azul e recalculo da média de estrelas da equipe.

#### **[F14] Prancheta Tática e Escalação Visual**
* **Objetivo:** Exibição gráfica do campo de futebol com a posição dos jogadores escalados em cada time.
* **Componentes / Páginas:** `Components/Teams/TacticalBoard.razor`
* **Services & DTOs:** `TacticalFormationDto`
* **Testes bUnit:** Testar a renderização dos pinos dos jogadores nas coordenadas X/Y do campo.

#### **[F15] Registro de Súmula Pós-Jogo (Estatísticas em Tempo Real)**
* **Objetivo:** Tela para o mesário/administrador registrar o placar, gols, assistências, cartões amarelos/vermelhos e minutos jogados.
* **Componentes / Páginas:** `Pages/Matches/MatchStatsSummary.razor`
* **Services & DTOs:** `IMatchApiService` (`RegisterMatchStatsAsync`), `MatchStatsDto`
* **Endpoints API Consumidos:** `POST /api/v1/matches/{id}/stats` (`RegisterMatchStatsCommand`)
* **Testes bUnit:** Incrementar/decrementar contadores de gols e assistências para um jogador e validar envio da súmula.

#### **[F16] Votação do Craque do Jogo (MVP)**
* **Objetivo:** Permite que os atletas que jogaram a partida votem no melhor jogador da rodada.
* **Componentes / Páginas:** `Pages/Matches/MvpVoting.razor`
* **Services & DTOs:** `IMatchApiService` (`SubmitMvpVoteAsync`), `SubmitMvpVoteDto`
* **Endpoints API Consumidos:** `POST /api/v1/matches/{id}/mvp-vote` (`SubmitMvpVoteCommand`)
* **Testes bUnit:** Garantir que o jogador não possa votar em si mesmo (se restrito) e que após votar o botão fique desabilitado.

#### **[F17] Rankings da Temporada e Histórico**
* **Objetivo:** Tabelas e gráficos com a classificação geral da temporada: Artilharia, Líderes de Assistência, Cartões, Assiduidade (Frequência) e Avaliação Média.
* **Componentes / Páginas:** `Pages/Reports/SeasonRankings.razor`
* **Services & DTOs:** `IReportApiService`, `SeasonRankingDto`
* **Endpoints API Consumidos:** `GET /api/v1/reports/rankings` (`GetSeasonRankingQuery`)
* **Testes bUnit:** Alternância entre abas de "Gols", "Assistências" e "Frequência", ordenando os dados corretamente.

---

### Fase 4: Módulo Financeiro & Arrecadação (`BabaPlay.Financial`)

#### **[F18] Dashboard Financeiro do Tenant**
* **Objetivo:** Visão consolidada de entradas, saídas, pendências de mensalidades e saldo da caixinha.
* **Componentes / Páginas:** `Pages/Financial/FinancialDashboard.razor`
* **Services & DTOs:** `IFinancialApiService`, `FinancialOverviewDto`
* **Endpoints API Consumidos:** `GET /api/v1/financial/overview`
* **Testes bUnit:** Testar a exibição dos balanços e indicadores visuais de déficit/superávit.

#### **[F19] Gestão de Mensalidades Recorrentes e Faturas**
* **Objetivo:** Listagem das faturas geradas para o atleta, status de pagamento (Pendente, Pago, Atrasado) e opção de emissão de cobrança.
* **Componentes / Páginas:** `Pages/Financial/InvoicesList.razor`
* **Services & DTOs:** `IFinancialApiService`, `InvoiceDto`
* **Endpoints API Consumidos:** `GET /api/v1/financial/invoices`
* **Testes bUnit:** Filtros por status de fatura e botão de geração manual de cobrança.

#### **[F20] Pagamento via Pix e Cartão de Crédito**
* **Objetivo:** Modal de checkout com exibição do QR Code Pix (Copia e Cola) e status de confirmação em tempo real.
* **Componentes / Páginas:** `Components/Financial/PixPaymentModal.razor`
* **Services & DTOs:** `IFinancialApiService` (`GetPixPaymentAsync`), `PixPaymentDetailsDto`
* **Endpoints API Consumidos:** `POST /api/v1/financial/invoices/{id}/pay-pix`
* **Testes bUnit:** Cópia da chave Pix para a área de transferência via Javascript Interop e escuta de pagamento efetuado.

#### **[F21] Controle de Inadimplência**
* **Objetivo:** Painel restrito a administradores para listar atletas inadimplentes e disparar lembretes de cobrança.
* **Componentes / Páginas:** `Pages/Financial/DefaultersReport.razor`
* **Services & DTOs:** `IFinancialApiService` (`GetDefaultersAsync`), `DefaulterMemberDto`
* **Endpoints API Consumidos:** `GET /api/v1/financial/defaulters` (`GetDefaultersListQuery`)
* **Testes bUnit:** Renderização dos dias de atraso e disparo do evento de lembrete.

#### **[F22] Prestação de Contas Pública (Balancete para Sócios)**
* **Objetivo:** Transparência financeira exibindo comprovantes de receitas e despesas registradas pela diretoria para todos os membros da associação.
* **Componentes / Páginas:** `Pages/Financial/FinancialStatement.razor`
* **Services & DTOs:** `IFinancialApiService`, `FinancialStatementDto`
* **Endpoints API Consumidos:** `GET /api/v1/financial/statement` (`GetFinancialStatementQuery`)
* **Testes bUnit:** Exibição da lista de despesas e cálculo total do saldo transparente.

#### **[F23] Caixinha do Time (Vaquinhas de Eventos)**
* **Objetivo:** Arrecadação colaborativa para eventos extras (ex: churrasco de fim de ano, compra de novos coletes), mostrando meta e progresso da arrecadação.
* **Componentes / Páginas:** `Pages/Financial/Fundraisers.razor`
* **Services & DTOs:** `IFinancialApiService`, `FundraiserDto`
* **Endpoints API Consumidos:** `GET /api/v1/financial/fundraisers`, `POST /api/v1/financial/fundraisers`
* **Testes bUnit:** Renderização da barra de progresso da meta (ex: R$ 500 / R$ 1.000).

---

### Fase 5: Módulo Comunicação & Real-Time (`BabaPlay.Communication`)

#### **[F24] Mural de Avisos e Comunicados**
* **Objetivo:** Espaço para a diretoria publicar comunicados oficiais com marcação de leitura pelos sócios.
* **Componentes / Páginas:** 
  * `Pages/Communication/Announcements.razor`
  * `Components/Communication/AnnouncementCard.razor`
* **Services & DTOs:** `ICommunicationApiService`, `AnnouncementDto`
* **Endpoints API Consumidos:** `GET /api/v1/communication/announcements`, `POST /api/v1/communication/announcements`
* **Testes bUnit:** Testar a publicação de um novo aviso e a badge de "Novo".

#### **[F25] Enquetes Interativas**
* **Objetivo:** Votações rápidas promovidas pela associação (ex: escolha do uniforme, definição de dia do jogo).
* **Componentes / Páginas:** `Pages/Communication/Polls.razor`
* **Services & DTOs:** `ICommunicationApiService`, `PollDto`, `SubmitPollVoteDto`
* **Endpoints API Consumidos:** `GET /api/v1/communication/polls`, `POST /api/v1/communication/polls/{id}/vote`
* **Testes bUnit:** Votação em uma opção e exibição da porcentagem de votos computada.

#### **[F26] Hub de Notificações no App**
* **Objetivo:** Central de alertas do usuário (alteração de horário de jogo, nova cobrança, convocação).
* **Componentes / Páginas:** `Components/Communication/NotificationCenter.razor`
* **Services & DTOs:** `INotificationApiService`, `NotificationDto`
* **Endpoints API Consumidos:** `GET /api/v1/notifications`, `PUT /api/v1/notifications/{id}/read`
* **Testes bUnit:** Marcar notificação como lida e atualizar contador de não lidas no cabeçalho.

#### **[F27] Chat em Tempo Real via SignalR**
* **Objetivo:** Chat da turma/time utilizando a biblioteca cliente do SignalR no Blazor (`Microsoft.AspNetCore.SignalR.Client`).
* **Componentes / Páginas:** `Pages/Communication/TeamChat.razor`
* **Services & DTOs:** `SignalRChatService.cs`, `ChatMessageDto`
* **Endpoints API / Hubs Consumidos:** `HubConnection` conectando em `/hubs/chat`
* **Testes bUnit:** Testar envio de mensagem e simulação de recepção de evento do Hub SignalR.

---

### Fase 6: Configurações do Tenant

#### **[F28] Configurações Gerais do Tenant & Personalização**
* **Objetivo:** Gerenciamento das preferências da associação (dia fixo do baba, limite de jogadores por time, regras de sorteio, upload de escudo e convites por link).
* **Componentes / Páginas:** `Pages/Settings/TenantSettings.razor`
* **Services & DTOs:** `ITenantApiService`, `TenantSettingsDto`
* **Endpoints API Consumidos:** `GET /api/v1/tenants/settings`, `PUT /api/v1/tenants/settings`
* **Testes bUnit:** Alteração das cores do tema no Blazor e atualização das regras do baba.

---

## 5. Estratégia de Testes (TDD com bUnit e xUnit)

Para cada funcionalidade criada no Blazor, a estrutura do projeto de testes deve ser mantida em `Backend/src/BabaPlay.Tests/Web/`:

```text
Backend/src/BabaPlay.Tests/Web/
├── Auth/
│   ├── LoginTests.cs
│   └── ResetPasswordTests.cs
├── Matches/
│   ├── MatchCheckinTests.cs
│   └── TeamDrawTests.cs
├── Helpers/
│   └── TestContextExtensions.cs
```

### Modelo de Teste bUnit Padrão:

```csharp
public class MatchCheckinTests : TestContext
{
    [Fact]
    public void SubmitRsvp_WhenClicked_ShouldCallApiServiceAndToggleStatus()
    {
        // Arrange
        var mockApiService = new Mock<IMatchApiService>();
        mockApiService
            .Setup(x => x.SubmitRsvpAsync(It.IsAny<Guid>(), It.IsAny<RsvpSubmissionDto>()))
            .ReturnsAsync(new RsvpResultDto { Success = true, IsConfirmed = true });

        Services.AddSingleton(mockApiService.Object);

        // Act
        var cut = RenderComponent<MatchCheckin>(parameters => parameters
            .Add(p => p.MatchId, Guid.NewGuid()));

        var confirmButton = cut.Find("button.btn-confirm");
        confirmButton.Click();

        // Assert
        mockApiService.Verify(x => x.SubmitRsvpAsync(It.IsAny<Guid>(), It.IsAny<RsvpSubmissionDto>()), Times.Once);
        Assert.Contains("Presença Confirmada", cut.Markup);
    }
}
```

---

## 6. Checklist de Aceitação por Função

Antes de considerar uma função do Blazor concluída:

1. [ ] **Componente Razors divididos:** Páginas (`Pages/`) limpas e componentes reutilizáveis (`Components/`) desacoplados.
2. [ ] **Services HTTP:** Nenhuma chamada direta de `HttpClient` inline nos componentes; tudo encapsulado nos `*ApiService.cs`.
3. [ ] **Validação de Formulários:** Uso de `EditForm` com `DataAnnotationsValidator` e mensagens de erro visíveis.
4. [ ] **Multitenancy Header:** Garantir que o `TenantId` é repassado nas chamadas via `AuthorizationHeaderHandler`.
5. [ ] **Tratamento de Exceções & Loading:** Indicador de carregamento (`LoadingSpinner`) exibido durante requisições e alertas para falhas HTTP (400, 401, 403, 500).
6. [ ] **Testes de Renderização Passing:** Testes bUnit cobrindo os cenários de sucesso, erro e interação.
