using System;

namespace BabaPlay.Web.Services.State;

public class TenantState
{
    public Guid? CurrentTenantId { get; private set; }
    public string? CurrentTenantName { get; private set; }
    public string? CurrentTenantSlug { get; private set; }

    public event Action? OnChange;

    public void SetTenant(Guid tenantId, string tenantName, string? tenantSlug = null)
    {
        CurrentTenantId = tenantId;
        CurrentTenantName = tenantName;
        CurrentTenantSlug = tenantSlug;
        NotifyStateChanged();
    }

    public void ClearTenant()
    {
        CurrentTenantId = null;
        CurrentTenantName = null;
        CurrentTenantSlug = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
