using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.ValueObjects;

public readonly record struct RankingPeriod(DateTime FromUtc, DateTime ToUtc)
{
    public static RankingPeriod Create(DateTime fromUtc, DateTime toUtc)
    {
        if (fromUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("FromUtc", "FromUtc must be UTC.");

        if (toUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("ToUtc", "ToUtc must be UTC.");

        if (fromUtc > toUtc)
            throw new ValidationException("Period", "FromUtc cannot be greater than ToUtc.");

        return new RankingPeriod(fromUtc, toUtc);
    }
}