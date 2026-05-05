using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using BabaPlay.Domain.Enums;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class FinancialIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;
    private static int _userIndex = 1;

    public FinancialIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, PlayerWebApplicationFactory.TestUserIds[0].ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserEmailHeader, "financial-test@babaplay.com");
    }

    [Fact]
    public async Task PostCashTransaction_ValidRequest_ShouldReturn201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/financial/cash-transaction", new
        {
            type = CashTransactionType.Income,
            amount = 120.50m,
            occurredOnUtc = DateTime.UtcNow,
            description = "Mensalidade avulsa",
            playerId = (Guid?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CashTransactionResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Type.Should().Be(CashTransactionType.Income);
    }

    [Fact]
    public async Task PostMonthlyFeeAndPayment_ThenReverse_ShouldReturn200()
    {
        var player = await CreatePlayerAsync();
        var dueDate = DateTime.UtcNow.Date.AddDays(7);

        var monthlyFeeResponse = await _client.PostAsJsonAsync("/api/v1/financial/monthly-fee", new
        {
            playerId = player.Id,
            year = dueDate.Year,
            month = dueDate.Month,
            amount = 85m,
            dueDateUtc = dueDate,
            notes = "Mensalidade regular",
        });

        monthlyFeeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var monthlyFee = await monthlyFeeResponse.Content.ReadFromJsonAsync<PlayerMonthlyFeeResponse>(JsonOptions);
        monthlyFee.Should().NotBeNull();

        var paymentResponse = await _client.PostAsJsonAsync("/api/v1/financial/monthly-fee-payment", new
        {
            monthlyFeeId = monthlyFee!.Id,
            amount = 85m,
            paidAtUtc = DateTime.UtcNow,
            notes = "Pagamento em dinheiro",
        });

        paymentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var payment = await paymentResponse.Content.ReadFromJsonAsync<MonthlyFeePaymentResponse>(JsonOptions);
        payment.Should().NotBeNull();

        var reverseResponse = await _client.PostAsJsonAsync($"/api/v1/financial/monthly-fee-payment/{payment!.Id}/reverse", new
        {
            reversedAtUtc = DateTime.UtcNow,
        });

        reverseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reversed = await reverseResponse.Content.ReadFromJsonAsync<MonthlyFeePaymentResponse>(JsonOptions);
        reversed.Should().NotBeNull();
        reversed!.IsReversed.Should().BeTrue();
    }

    [Fact]
    public async Task GetDelinquency_WithOverdueFee_ShouldReturn200()
    {
        var player = await CreatePlayerAsync();
        var overdueDate = DateTime.UtcNow.Date.AddDays(-15);

        var createFeeResponse = await _client.PostAsJsonAsync("/api/v1/financial/monthly-fee", new
        {
            playerId = player.Id,
            year = overdueDate.Year,
            month = overdueDate.Month,
            amount = 100m,
            dueDateUtc = overdueDate,
            notes = "Em atraso",
        });

        createFeeResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _client.GetAsync($"/api/v1/financial/delinquency?referenceUtc={Uri.EscapeDataString(DateTime.UtcNow.ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DelinquencyResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMonthlySummary_ValidPeriod_ShouldReturn200()
    {
        var now = DateTime.UtcNow;
        var response = await _client.GetAsync($"/api/v1/financial/monthly-summary?year={now.Year}&month={now.Month}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MonthlySummaryResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Year.Should().Be(now.Year);
        body.Month.Should().Be(now.Month);
    }

    private async Task<PlayerResponse> CreatePlayerAsync()
    {
        var index = Interlocked.Increment(ref _userIndex);
        var userId = PlayerWebApplicationFactory.TestUserIds[index % PlayerWebApplicationFactory.TestUserIds.Length];
        var response = await _client.PostAsJsonAsync("/api/v1/player", new
        {
            userId,
            name = $"Financial Player {index}",
            nickname = "Financeiro",
            phone = "11999999999",
            dateOfBirth = "1994-05-07",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var player = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        player.Should().NotBeNull();
        return player!;
    }
}
