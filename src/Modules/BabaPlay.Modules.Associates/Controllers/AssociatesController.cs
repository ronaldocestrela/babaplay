using BabaPlay.Modules.Associates.Services;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BabaPlay.Modules.Associates.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class AssociatesController(
    AssociateService service,
    IAssociateInvitationService invitations,
    IOptions<InvitationLinkOptions> invitationOptions) : BaseController
{
    private readonly AssociateService _service = service;
    private readonly IAssociateInvitationService _invitations = invitations;
    private readonly InvitationLinkOptions _invitationOptions = invitationOptions.Value;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        FromResult(await _service.ListAsync(ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        FromResult(await _service.GetAsync(id, ct));

    public sealed record AssociateBody(string Name, string? Email, string? Phone, IReadOnlyList<string> PositionIds);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssociateBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.Name, body.Email, body.Phone, body.PositionIds, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AssociateBody body, CancellationToken ct) =>
        FromResult(await _service.UpdateAsync(id, body.Name, body.Email, body.Phone, body.PositionIds, ct));

    public sealed record SetActiveBody(bool IsActive);

    [HttpPatch("{id}/active")]
    public async Task<IActionResult> SetActive(string id, [FromBody] SetActiveBody body, CancellationToken ct) =>
        FromResult(await _service.SetActiveAsync(id, body.IsActive, ct));

    public sealed record CreateInvitationBody(string? Email = null, bool IsSingleUse = false);

    public sealed record InvitationResponse(string Token, string? Email, bool IsSingleUse, DateTime ExpiresAt, string Link);

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("invitations")]
    public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationBody body, CancellationToken ct)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return FromResult(Result.Unauthorized<InvitationResponse>("Authenticated user is required."));

        var tenantSubdomain = GetTenantSubdomain();
        if (string.IsNullOrWhiteSpace(tenantSubdomain))
            return FromResult(Result.Invalid<InvitationResponse>(
                "Tenant subdomain could not be resolved; send X-Tenant-Subdomain or use a tenant host."));

        var invitation = await _invitations.CreateAsync(body.Email, body.IsSingleUse, userId, TimeSpan.FromDays(7), ct);
        if (invitation.IsFailure)
            return FromResult(FailFromResult<InvitationResponse>(invitation));

        var payload = new InvitationResponse(
            invitation.Value.Token,
            invitation.Value.Email,
            body.IsSingleUse,
            invitation.Value.ExpiresAt,
            BuildInvitationLink(invitation.Value.Token, tenantSubdomain));

        return FromResult(Result.Success(payload));
    }

    public sealed record InvitationValidationResponse(string Token, string? Email, bool IsSingleUse, DateTime ExpiresAt);

    [AllowAnonymous]
    [HttpGet("invitations/{token}")]
    public async Task<IActionResult> ValidateInvitation(string token, CancellationToken ct)
    {
        var invitation = await _invitations.ValidateAsync(token, ct);
        if (invitation.IsFailure)
            return FromResult(FailFromResult<InvitationValidationResponse>(invitation));

        var payload = new InvitationValidationResponse(
            invitation.Value.Token,
            invitation.Value.Email,
            invitation.Value.IsSingleUse,
            invitation.Value.ExpiresAt);

        return FromResult(Result.Success(payload));
    }

    private static string BuildInvitationLink(string token, string tenantSubdomain, string baseUrl)
    {
        var path = $"/convite/{Uri.EscapeDataString(token)}";
        var query = $"tenant={Uri.EscapeDataString(tenantSubdomain)}";
        return $"{baseUrl.TrimEnd('/')}{path}?{query}";
    }

    private string BuildInvitationLink(string token, string tenantSubdomain)
    {
        var baseUrl = _invitationOptions.FrontendBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = $"{Request.Scheme}://{Request.Host.Value}";

        return BuildInvitationLink(token, tenantSubdomain, baseUrl);
    }

    private static Result<T> FailFromResult<T>(Result result)
    {
        if (result.Errors.Count > 0)
            return Result.Fail<T>(result.Errors, result.Status);

        if (!string.IsNullOrWhiteSpace(result.Error))
            return Result.Fail<T>(result.Error, result.Status);

        return Result.Fail<T>("Operation failed.", result.Status);
    }
}
