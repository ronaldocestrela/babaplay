using Microsoft.AspNetCore.Identity;

namespace BabaPlay.Infrastructure.Entities;

/// <summary>
/// Extends the default IdentityUser with application-specific properties.
/// Lives in Infrastructure because it is tightly coupled to ASP.NET Identity's persistence model.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
