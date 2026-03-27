using BabaPlay.Modules.Financial.Services;
using BabaPlay.SharedKernel.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Modules.Financial.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class MembershipsController : BaseController
{
    private readonly MembershipService _service;

    public MembershipsController(MembershipService service) => _service = service;

    [HttpGet("associate/{associateId}")]
    public async Task<IActionResult> ForAssociate(string associateId, CancellationToken ct) =>
        FromResult(await _service.ListForAssociateAsync(associateId, ct));

    public sealed record MembershipBody(string AssociateId, int Year, int Month, decimal Amount);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MembershipBody body, CancellationToken ct) =>
        FromResult(await _service.CreateAsync(body.AssociateId, body.Year, body.Month, body.Amount, ct));

    public sealed record PaymentBody(decimal Amount, string Method);

    [HttpPost("{membershipId}/payments")]
    public async Task<IActionResult> Pay(string membershipId, [FromBody] PaymentBody body, CancellationToken ct) =>
        FromResult(await _service.RegisterPaymentAsync(membershipId, body.Amount, body.Method, ct));
}
