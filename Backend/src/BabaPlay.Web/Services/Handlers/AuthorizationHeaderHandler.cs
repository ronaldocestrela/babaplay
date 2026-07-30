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

        if (_tenantState.CurrentTenantId.HasValue)
        {
            request.Headers.Remove("X-Tenant-Id");
            request.Headers.Add("X-Tenant-Id", _tenantState.CurrentTenantId.Value.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
