namespace BabaPlay.Web.Serialization;

/// <summary>
/// Mirror of <c>BabaPlay.Domain.Enums.MatchStatus</c> for JSON deserialization in the Web client.
/// </summary>
internal enum WebMatchStatus
{
    Pending = 0,
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
}
