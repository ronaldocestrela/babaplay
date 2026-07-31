using System;
using System.Collections.Generic;
using System.Linq;

namespace BabaPlay.Web.Services.State;

public class TenantState
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public Guid? CurrentTenantId { get; private set; }
    public string? CurrentTenantName { get; private set; }
    public string? CurrentTenantSlug { get; private set; }
    public bool IsOwner { get; private set; }
    public IReadOnlyCollection<string> Permissions => _permissions;

    public event Action? OnChange;

    public void SetTenant(Guid tenantId, string tenantName, string? tenantSlug = null, bool isOwner = false)
    {
        CurrentTenantId = tenantId;
        CurrentTenantName = tenantName;
        CurrentTenantSlug = tenantSlug;
        IsOwner = isOwner;
        NotifyStateChanged();
    }

    public void SetPermissions(IEnumerable<string>? permissionCodes)
    {
        _permissions.Clear();
        if (permissionCodes is not null)
        {
            foreach (var code in permissionCodes.Where(c => !string.IsNullOrWhiteSpace(c)))
                _permissions.Add(code);
        }

        NotifyStateChanged();
    }

    public bool HasPermission(string permissionCode)
        => !string.IsNullOrWhiteSpace(permissionCode)
           && _permissions.Contains(permissionCode);

    public void ClearTenant()
    {
        CurrentTenantId = null;
        CurrentTenantName = null;
        CurrentTenantSlug = null;
        IsOwner = false;
        _permissions.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
