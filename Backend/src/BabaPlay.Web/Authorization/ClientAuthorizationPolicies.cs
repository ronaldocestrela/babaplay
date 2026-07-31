namespace BabaPlay.Web.Authorization;

/// <summary>
/// Named authorization policies used by Blazor <c>AuthorizeView</c> components.
/// Names mirror the API policies in <c>AuthorizationPolicyNames</c>.
/// </summary>
public static class ClientAuthorizationPolicies
{
    public const string CommunicationWrite = "CommunicationWrite";
}
