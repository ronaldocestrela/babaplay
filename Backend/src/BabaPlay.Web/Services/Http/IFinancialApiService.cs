using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface IFinancialApiService
{
    Task<FinancialOverviewDto?> GetFinancialOverviewAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);

    Task<List<InvoiceDto>?> GetInvoicesAsync(int? year = null, int? month = null, Guid? playerId = null, int? status = null, CancellationToken cancellationToken = default);

    Task<InvoiceDto?> CreateInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default);

    Task<PixPaymentDetailsDto?> GetPixPaymentAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    Task<bool> ConfirmPixPaymentAsync(Guid invoiceId, string? txId = null, CancellationToken cancellationToken = default);

    Task<DefaultersListDto?> GetDefaultersAsync(DateTime? referenceUtc = null, CancellationToken cancellationToken = default);

    Task<bool> SendPaymentReminderAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<FinancialStatementDto?> GetFinancialStatementAsync(int? year = null, int? month = null, CancellationToken cancellationToken = default);
}
