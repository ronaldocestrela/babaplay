namespace Application.Features.Cors;

using Application.Features.Cors.Models;

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
