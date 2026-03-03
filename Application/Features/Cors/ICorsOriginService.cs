using BabaPlayShared.Library.Models.Requests.Cors;
using BabaPlayShared.Library.Models.Responses.Cors;

namespace Application.Features.Cors;

public interface ICorsOriginService
{
    Task<List<CorsOriginResponse>> GetAllAsync();
    Task<CorsOriginResponse?> GetByIdAsync(string id);
    Task<CorsOriginResponse> CreateAsync(CreateCorsOriginRequest request);
    Task<CorsOriginResponse> UpdateAsync(string id, UpdateCorsOriginRequest request);
    Task<bool> DeleteAsync(string id);
    Task<List<string>> GetAllowedOriginsAsync();
    void ClearCache();
}
