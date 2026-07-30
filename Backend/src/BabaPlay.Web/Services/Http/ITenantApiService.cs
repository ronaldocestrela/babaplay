using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using BabaPlay.Web.Models;

namespace BabaPlay.Web.Services.Http;

public interface ITenantApiService
{
    Task<IReadOnlyList<TenantSummaryDto>> GetMyMembershipsAsync(CancellationToken cancellationToken = default);
    Task<TenantSummaryDto?> CreateTenantAsync(CreateTenantDto dto, IBrowserFile? logoFile, CancellationToken cancellationToken = default);
    Task<InviteValidationDto?> ValidateInviteAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> AcceptInviteAsync(AcceptInviteDto dto, CancellationToken cancellationToken = default);
}
