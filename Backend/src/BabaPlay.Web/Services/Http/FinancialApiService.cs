using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public sealed class FinancialApiService : IFinancialApiService
{
    private readonly HttpClient _httpClient;

    public FinancialApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiServiceResult<FinancialOverviewDto>> GetFinancialOverviewAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/financial/overview";
        if (year.HasValue && month.HasValue)
        {
            url += $"?year={year.Value}&month={month.Value}";
        }
        else if (year.HasValue)
        {
            url += $"?year={year.Value}";
        }
        else if (month.HasValue)
        {
            url += $"?month={month.Value}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiServiceResult<FinancialOverviewDto>.Fail(
                (int)response.StatusCode,
                FinancialApiErrorMessages.FromStatusCode(response.StatusCode));
        }

        var data = await response.Content.ReadFromJsonAsync<FinancialOverviewDto>(cancellationToken: cancellationToken);
        return ApiServiceResult<FinancialOverviewDto>.Ok(data!);
    }

    public async Task<List<InvoiceDto>?> GetInvoicesAsync(
        int? year = null,
        int? month = null,
        Guid? playerId = null,
        int? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (year.HasValue && year.Value > 0) queryParams.Add($"year={year.Value}");
        if (month.HasValue && month.Value > 0) queryParams.Add($"month={month.Value}");
        if (playerId.HasValue && playerId.Value != Guid.Empty) queryParams.Add($"playerId={playerId.Value}");
        if (status.HasValue) queryParams.Add($"status={status.Value}");

        var url = "api/v1/financial/invoices";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<List<InvoiceDto>>(cancellationToken: cancellationToken);
    }

    public async Task<InvoiceDto?> CreateInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            playerId = dto.PlayerId,
            year = dto.Year,
            month = dto.Month,
            amount = dto.Amount,
            dueDateUtc = dto.DueDateUtc.ToUniversalTime(),
            notes = dto.Notes
        };

        var response = await _httpClient.PostAsJsonAsync("api/v1/financial/monthly-fee", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: cancellationToken);
    }

    public async Task<PixPaymentDetailsDto?> GetPixPaymentAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/v1/financial/invoices/{invoiceId}/pay-pix", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PixPaymentDetailsDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> ConfirmPixPaymentAsync(Guid invoiceId, string? txId = null, CancellationToken cancellationToken = default)
    {
        var payload = new { txId };
        var response = await _httpClient.PostAsJsonAsync($"api/v1/financial/invoices/{invoiceId}/confirm-pix", payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<DefaultersListDto?> GetDefaultersAsync(DateTime? referenceUtc = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/financial/defaulters";
        if (referenceUtc.HasValue)
        {
            url += $"?referenceUtc={Uri.EscapeDataString(referenceUtc.Value.ToString("o"))}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<DefaultersListDto>(cancellationToken: cancellationToken);
    }

    public async Task<bool> SendPaymentReminderAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/v1/financial/defaulters/{playerId}/remind", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<FinancialStatementDto?> GetFinancialStatementAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/financial/statement";
        if (year.HasValue && month.HasValue)
        {
            url += $"?year={year.Value}&month={month.Value}";
        }
        else if (year.HasValue)
        {
            url += $"?year={year.Value}";
        }
        else if (month.HasValue)
        {
            url += $"?month={month.Value}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<FinancialStatementDto>(cancellationToken: cancellationToken);
    }

    public async Task<List<FundraiserDto>?> GetFundraisersAsync(bool? onlyActive = null, CancellationToken cancellationToken = default)
    {
        var url = "api/v1/financial/fundraisers";
        if (onlyActive.HasValue)
        {
            url += $"?onlyActive={onlyActive.Value.ToString().ToLowerInvariant()}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<List<FundraiserDto>>(cancellationToken: cancellationToken);
    }

    public async Task<FundraiserDto?> CreateFundraiserAsync(CreateFundraiserDto dto, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            title = dto.Title,
            description = dto.Description,
            targetAmount = dto.TargetAmount,
            deadlineUtc = dto.DeadlineUtc?.ToUniversalTime()
        };

        var response = await _httpClient.PostAsJsonAsync("api/v1/financial/fundraisers", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<FundraiserDto>(cancellationToken: cancellationToken);
    }

    public async Task<FundraiserDto?> ContributeToFundraiserAsync(Guid id, ContributeFundraiserDto dto, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            amount = dto.Amount,
            playerId = dto.PlayerId,
            contributorName = dto.ContributorName
        };

        var response = await _httpClient.PostAsJsonAsync($"api/v1/financial/fundraisers/{id}/contribute", payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<FundraiserDto>(cancellationToken: cancellationToken);
    }
}
