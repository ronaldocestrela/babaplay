using BabaPlay.SharedKernel.Entities;

namespace BabaPlay.Modules.Platform.Entities;

/// <summary>
/// CORS origins. TenantId null = platform-wide.
/// </summary>
public class AllowedOrigin : BaseEntity
{
    public string Origin { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}
