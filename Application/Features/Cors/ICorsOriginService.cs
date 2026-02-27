namespace Application.Features.Cors;

public interface ICorsOriginService
{
    Task<List<string>> GetAllowedOriginsAsync();
    void ClearCache();
}
