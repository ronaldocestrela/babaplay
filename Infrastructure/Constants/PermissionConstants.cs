using System.Collections.ObjectModel;

namespace Infrastructure.Constants;

public static class AssociationAction
{
    public const string Read = nameof(Read);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string RefreshToken = nameof(RefreshToken);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class AssociationFeature
{
    public const string Tenants = nameof(Tenants);
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Associations = nameof(Associations);
    public const string Tokens = nameof(Tokens);
}

public record AssociationPermission(string Action, string Feature, string Description, string Group, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Feature);

    public static string NameFor(string action, string feature) => $"Permission.{feature}.{action}";
}

public static class AssociationPermissions
{
    private static readonly AssociationPermission[] _allPermissions =
    [
        new AssociationPermission(AssociationAction.Create, AssociationFeature.Tenants, "Create Tenants", "Tenancy", IsRoot: true),
        new AssociationPermission(AssociationAction.Read, AssociationFeature.Tenants, "Read Tenants", "Tenancy", IsRoot: true),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.Tenants, "Update Tenants", "Tenancy", IsRoot: true),
        new AssociationPermission(AssociationAction.UpgradeSubscription, AssociationFeature.Tenants, "Upgrade Tenant's Subscription", "Tenancy", IsRoot: true),

        new AssociationPermission(AssociationAction.Create, AssociationFeature.Users, "Create Users", "SystemAccess"),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.Users, "Update Users", "SystemAccess"),
        new AssociationPermission(AssociationAction.Delete, AssociationFeature.Users, "Delete Users", "SystemAccess"),
        new AssociationPermission(AssociationAction.Read, AssociationFeature.Users, "Read Users", "SystemAccess"),

        new AssociationPermission(AssociationAction.Read, AssociationFeature.UserRoles, "Read User Roles", "SystemAccess"),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.UserRoles, "Update User Roles", "SystemAccess"),

        new AssociationPermission(AssociationAction.Read, AssociationFeature.Roles, "Read Roles", "SystemAccess"),
        new AssociationPermission(AssociationAction.Create, AssociationFeature.Roles, "Create Roles", "SystemAccess"),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.Roles, "Update Roles", "SystemAccess"),
        new AssociationPermission(AssociationAction.Delete, AssociationFeature.Roles, "Delete Roles", "SystemAccess"),

        new AssociationPermission(AssociationAction.Read, AssociationFeature.RoleClaims, "Read Role Claims/Permissions", "SystemAccess"),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.RoleClaims, "Update Role Claims/Permissions", "SystemAccess"),

        new AssociationPermission(AssociationAction.Read, AssociationFeature.Associations, "Read Associations", "Academics", IsBasic: true),
        new AssociationPermission(AssociationAction.Create, AssociationFeature.Associations, "Create Associations", "Academics"),
        new AssociationPermission(AssociationAction.Update, AssociationFeature.Associations, "Update Associations", "Academics"),
        new AssociationPermission(AssociationAction.Delete, AssociationFeature.Associations, "Delete Associations", "Academics"),

        new AssociationPermission(AssociationAction.RefreshToken, AssociationFeature.Tokens, "Generate Refresh Token", "SystemAccess", IsBasic: true)
    ];

    public static IReadOnlyList<AssociationPermission> All { get; } 
            = new ReadOnlyCollection<AssociationPermission>(_allPermissions);

        public static IReadOnlyList<AssociationPermission> Root { get; } 
            = new ReadOnlyCollection<AssociationPermission>(_allPermissions.Where(p => p.IsRoot).ToArray());

        public static IReadOnlyList<AssociationPermission> Admin { get; } 
            = new ReadOnlyCollection<AssociationPermission>(_allPermissions.Where(p => !p.IsRoot).ToArray());

        public static IReadOnlyList<AssociationPermission> Basic { get; } 
            = new ReadOnlyCollection<AssociationPermission>(_allPermissions.Where(p => p.IsBasic).ToArray());
}
