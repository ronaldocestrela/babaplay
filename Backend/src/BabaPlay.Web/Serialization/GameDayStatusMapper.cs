namespace BabaPlay.Web.Serialization;

public static class GameDayStatusMapper
{
    public static bool TryToNumeric(string status, out int value)
    {
        if (Enum.TryParse<WebGameDayStatus>(status, ignoreCase: true, out var parsed))
        {
            value = (int)parsed;
            return true;
        }

        value = 0;
        return false;
    }

    public static string? GetNextStatus(string currentStatus)
        => currentStatus switch
        {
            "Pending" => "Confirmed",
            "Confirmed" => "Completed",
            _ => null,
        };
}
