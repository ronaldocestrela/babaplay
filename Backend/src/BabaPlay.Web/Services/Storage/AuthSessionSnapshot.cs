using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Services.Storage;

public sealed class AuthSessionSnapshot
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public List<string> Roles { get; set; } = new();
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? TenantSlug { get; set; }
    public bool TenantIsOwner { get; set; }
}
