namespace BabaPlay.Infrastructure.Settings;

public sealed class MatchSummaryStorageSettings
{
    public const string SectionName = "MatchSummaryStorage";

    public string RootPath { get; init; } = "storage";
}
