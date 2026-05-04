namespace BabaPlay.Application.Common;

/// <summary>
/// RBAC permission codes and default role matrix used during tenant provisioning.
/// </summary>
public static class RbacCatalog
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Member = "Member";
        public const string Viewer = "Viewer";
    }

    public static class Permissions
    {
        public const string RbacRolesRead = "rbac.roles.read";
        public const string RbacRolesWrite = "rbac.roles.write";
        public const string RbacRolesAssign = "rbac.roles.assign";
        public const string RbacPermissionsWrite = "rbac.permissions.write";

        public const string PlayersRead = "players.read";
        public const string PlayersWrite = "players.write";
        public const string TenantRead = "tenant.read";
        public const string MatchesRead = "matches.read";
        public const string MatchesWrite = "matches.write";
        public const string MatchEventsRead = "matchevents.read";
        public const string MatchEventsWrite = "matchevents.write";
        public const string MatchEventTypesRead = "matcheventtypes.read";
        public const string MatchEventTypesWrite = "matcheventtypes.write";
        public const string RankingRead = "ranking.read";
        public const string RankingWrite = "ranking.write";
    }

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Roles.Admin] =
            [
                Permissions.RbacRolesRead,
                Permissions.RbacRolesWrite,
                Permissions.RbacRolesAssign,
                Permissions.RbacPermissionsWrite,
                Permissions.PlayersRead,
                Permissions.PlayersWrite,
                Permissions.TenantRead,
                Permissions.MatchesRead,
                Permissions.MatchesWrite,
                Permissions.MatchEventsRead,
                Permissions.MatchEventsWrite,
                Permissions.MatchEventTypesRead,
                Permissions.MatchEventTypesWrite,
                Permissions.RankingRead,
                Permissions.RankingWrite,
            ],
            [Roles.Manager] =
            [
                Permissions.RbacRolesRead,
                Permissions.PlayersRead,
                Permissions.PlayersWrite,
                Permissions.TenantRead,
                Permissions.MatchesRead,
                Permissions.MatchesWrite,
                Permissions.MatchEventsRead,
                Permissions.MatchEventsWrite,
                Permissions.MatchEventTypesRead,
                Permissions.RankingRead,
                Permissions.RankingWrite,
            ],
            [Roles.Member] =
            [
                Permissions.PlayersRead,
                Permissions.TenantRead,
                Permissions.MatchesRead,
                Permissions.MatchEventsRead,
                Permissions.RankingRead,
            ],
            [Roles.Viewer] =
            [
                Permissions.PlayersRead,
                Permissions.TenantRead,
                Permissions.MatchesRead,
                Permissions.MatchEventsRead,
                Permissions.RankingRead,
            ],
        };

    public static IReadOnlyList<string> AllPermissions { get; } =
        DefaultRolePermissions.Values
            .SelectMany(x => x)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
