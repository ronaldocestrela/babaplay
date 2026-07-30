# Mapeamento de Funcionalidades (Functions & Features) - Baba Play

## Instruções para o LLM
Este documento lista todas as funcionalidades de negócio (features) que compõem o SaaS **Baba Play**. 
Ao implementar qualquer uma dessas funcionalidades, você deve mapeá-las para a arquitetura **CQRS (Commands para escrita, Queries para leitura)**, respeitando a separação por **Bounded Contexts (Módulos)** definida no arquivo `architecture.md` e utilizando os padrões descritos em `AGENTS.md`.

> [!NOTE]
> Para a construção e migração do Front-end em **Blazor (.NET 10)**, siga o passo a passo sequencial por função em [blazor_functions_guide.md](file:///home/rony/LPR/babaplay/blazor_functions_guide.md).

---

## 1. Módulo: `BabaPlay.Identity` (Administração, Acessos e Multitenancy)
Este módulo gerencia quem usa o sistema e a qual associação (Tenant) a pessoa pertence.

### Funcionalidades a serem construídas:
*   **Onboarding de Associações:** Cadastro de um novo time/liga (Tenant), configurando cores, escudo (White Label) e dados básicos.
*   **Gestão de Membros:** Cadastro completo de atletas, comissão técnica e dependentes/responsáveis.
*   **Controle de Acessos (RBAC):** Atribuição de perfis (Administrador, Treinador, Atleta, Responsável).
*   **Carteirinha Digital:** Geração e visualização do documento virtual do associado.

### Exemplos de Mapeamento CQRS:
*   `CreateTenantCommand`: Inicializa uma nova associação no banco.
*   `RegisterMemberCommand`: Adiciona um novo usuário vinculado ao `TenantId` atual.
*   `UpdateMemberRoleCommand`: Modifica as permissões de um usuário.
*   `GetTenantDashboardQuery`: Retorna os dados gerais (ativos, inativos) da associação.
*   `GetDigitalIdCardQuery`: Retorna os dados necessários para renderizar a carteirinha.

---

## 2. Módulo: `BabaPlay.Sports` (Gestão Esportiva, Logística e Estatísticas)
O "coração" do aplicativo, responsável pelo engajamento dos atletas e organização dos jogos.

### Funcionalidades a serem construídas:
*   **Calendário de Eventos:** Criação de jogos, treinos e eventos sociais.
*   **RSVP (Confirmação de Presença):** Sistema onde o atleta confirma se vai ou não ao jogo.
*   **Sorteio Inteligente de Times:** Algoritmo que balanceia os times (coletes) automaticamente com base nas notas prévias e posições dos jogadores confirmados.
*   **Prancheta Tática:** Definição visual do esquema tático e escalação.
*   **Registro Pós-Jogo (Súmula):** Inserção de gols, assistências, cartões, e minutos jogados.
*   **Craque do Jogo (MVP):** Sistema de votação aberto aos jogadores após a partida.
*   **Rankings e Histórico:** Geração de tabelas de artilharia, assistência e assiduidade (ranking da temporada).

### Exemplos de Mapeamento CQRS:
*   `ScheduleMatchCommand`: Cria um novo evento esportivo.
*   `SubmitRsvpCommand`: Registra a presença ou ausência de um atleta.
*   `GenerateBalancedTeamsCommand`: Executa o algoritmo de sorteio e salva os times formados.
*   `RegisterMatchStatsCommand`: Salva os eventos ocorridos na partida (gols, cartões).
*   `SubmitMvpVoteCommand`: Registra o voto de um atleta no craque do jogo.
*   `GetMatchRsvpListQuery`: Retorna a lista de confirmados e ausentes de um jogo.
*   `GetSeasonRankingQuery`: Retorna a tabela de artilheiros e líderes de assistência.

---

## 3. Módulo: `BabaPlay.Financial` (Financeiro e Arrecadação)
Responsável por garantir o fluxo de caixa do Tenant e reduzir a inadimplência.

### Funcionalidades a serem construídas:
*   **Mensalidades Recorrentes:** Geração automática de cobranças mensais para sócios.
*   **Pagamentos e Integração:** Processamento de pagamentos avulsos ou mensais via Pix e Cartão de Crédito.
*   **Controle de Inadimplência:** Dashboard para o administrador visualizar quem está em dia e quem está devendo.
*   **Prestação de Contas:** Tela pública (para os sócios) detalhando receitas e despesas da associação.
*   **Caixinha do Time:** Arrecadação de fundos colaborativos (vaquinha) para eventos específicos (ex: churrasco, aluguel de campo).

### Exemplos de Mapeamento CQRS:
*   `GenerateMonthlyInvoicesCommand`: Roda em background para criar as cobranças do mês.
*   `ProcessPaymentWebhookCommand`: Recebe o callback do gateway (ex: Stripe/MercadoPago) e baixa a fatura.
*   `RegisterExpenseCommand`: Administrador registra um gasto (ex: compra de bolas).
*   `GetDefaultersListQuery`: Retorna a lista de atletas com pagamentos atrasados.
*   `GetFinancialStatementQuery`: Retorna o balancete de receitas e despesas para transparência.

---

## 4. Módulo: `BabaPlay.Communication` (Notificações e Engajamento)
Garante que a comunicação seja centralizada, substituindo grupos desorganizados de WhatsApp.

### Funcionalidades a serem construídas:
*   **Mural de Avisos:** Espaço para a diretoria publicar comunicados oficiais.
*   **Enquetes:** Criação de votações rápidas (ex: "Qual a cor do novo uniforme?").
*   **Notificações Push / Alertas:** Disparo de avisos de alteração de horário de jogos, cobranças ou fechamento de RSVP.
*   **Chat Integrado:** Bate-papo em tempo real entre a equipe (usando SignalR).

### Exemplos de Mapeamento CQRS:
*   `PublishAnnouncementCommand`: Cria um novo aviso no mural.
*   `CreatePollCommand`: Inicia uma nova votação.
*   `SendChatMessageCommand`: Envia uma mensagem via WebSocket/SignalR.
*   `GetTeamAnnouncementsQuery`: Lista os comunicados ativos da associação.