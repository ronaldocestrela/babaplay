namespace BabaPlay.Web.Serialization;

/// <summary>
/// Mirror of <c>BabaPlay.Domain.Enums.GameDayStatus</c> for JSON deserialization in the Web client.
/// </summary>
internal enum WebGameDayStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
}
