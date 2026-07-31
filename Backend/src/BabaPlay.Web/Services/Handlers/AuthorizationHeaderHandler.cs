using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Web.Services.Handlers;

public class AuthorizationHeaderHandler : DelegatingHandler
{
    private readonly UserSessionState _userSessionState;
    private readonly TenantState _tenantState;

    public AuthorizationHeaderHandler(UserSessionState userSessionState, TenantState tenantState)
    {
        _userSessionState = userSessionState;
        _tenantState = tenantState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_userSessionState.IsAuthenticated && !string.IsNullOrWhiteSpace(_userSessionState.JwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userSessionState.JwtToken);
        }

        if (!string.IsNullOrWhiteSpace(_tenantState.CurrentTenantSlug))
        {
            request.Headers.Remove("X-Tenant-Slug");
            request.Headers.Add("X-Tenant-Slug", _tenantState.CurrentTenantSlug);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
