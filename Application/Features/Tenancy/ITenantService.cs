using BabaPlayShared.Library.Models.Requests.Tenancy;
using BabaPlayShared.Library.Models.Responses.Tenency;

namespace Application.Features.Tenancy;

public interface ITenantService
{
    Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct);
    Task<string> SignupAsync(SignupRequest signupRequest, CancellationToken ct);
    Task<string> ActivateAsync(string id);
    Task<string> DeactivateAsync(string id);
    Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription);
    Task<List<TenantResponse>> GetTenantsAsync();
    Task<TenantResponse> GetTenantByIdAsync(string id);
}
