namespace BabaPlay.Application.Common;

/// <summary>
/// Named authorization policies used by API endpoints.
/// </summary>
public static class AuthorizationPolicyNames
{
    public const string TenantMember = "TenantMember";
    public const string RbacRolesRead = "RbacRolesRead";
    public const string RbacRolesWrite = "RbacRolesWrite";
    public const string RbacRolesAssign = "RbacRolesAssign";
    public const string RbacPermissionsWrite = "RbacPermissionsWrite";
    public const string MatchesRead = "MatchesRead";
    public const string MatchesWrite = "MatchesWrite";
    public const string MatchEventsRead = "MatchEventsRead";
    public const string MatchEventsWrite = "MatchEventsWrite";
    public const string MatchEventTypesRead = "MatchEventTypesRead";
    public const string MatchEventTypesWrite = "MatchEventTypesWrite";
}
