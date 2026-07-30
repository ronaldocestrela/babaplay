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
}
