using Application.Features.Identity.Tokens.Queries;
using BabaPlayShared.Library.Constants;
using BabaPlayShared.Library.Models.Requests.Token;
using Infrastructure.Identity.Auth;
using Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace WebApi.Controllers;

public class TokenController : BaseApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [TenantHeader]
    [OpenApiOperation("Used to obtain jwt for login.")]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequest tokenRequest)
    {
        var response = await Sender.Send(new GetTokenQuery { TokenRequest = tokenRequest });

        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("refresh-token")]
    [OpenApiOperation("Used to generate new jwt from refresh token.")]
    [ShouldHavePermission(action: AssociationAction.RefreshToken, feature: AssociationFeature.Tokens)]
    public async Task<IActionResult> GetRefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var response = await Sender.Send(new GetRefreshTokenQuery { RefreshToken = refreshTokenRequest });
        if (response.IsSuccessful)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
}
