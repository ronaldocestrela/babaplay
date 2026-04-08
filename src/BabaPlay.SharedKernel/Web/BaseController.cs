using System.Security.Claims;
using BabaPlay.SharedKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.SharedKernel.Web;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<object?>.OkEmpty());

        return MapFailure(result.Status, result.Error, result.Errors);
    }

    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value));

        return MapFailure(result.Status, result.Error, result.Errors);
    }

    protected string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");

    /// <summary>
    /// Resolves the tenant slug (header <c>X-Tenant-Subdomain</c>, query <c>tenant</c>, or host subdomain) — same as tenant middleware.
    /// </summary>
    protected string? GetTenantSubdomain() => TenantSlugResolver.Resolve(Request);

    private IActionResult MapFailure(ResultStatus status, string? error, IReadOnlyList<string> errors)
    {
        var body = ApiResponse<object?>.Fail(error, errors);
        return status switch
        {
            ResultStatus.NotFound => NotFound(body),
            ResultStatus.Invalid => BadRequest(body),
            ResultStatus.Conflict => Conflict(body),
            ResultStatus.Unauthorized => Unauthorized(body),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
            _ => BadRequest(body)
        };
    }
}
