using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.ValueObjects;

public readonly record struct BillingCompetence(int Year, int Month)
{
    public string Display => $"{Year:D4}-{Month:D2}";

    public static BillingCompetence Create(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ValidationException("Year", "Year must be between 2000 and 2100.");

        if (month < 1 || month > 12)
            throw new ValidationException("Month", "Month must be between 1 and 12.");

        return new BillingCompetence(year, month);
    }

    public static BillingCompetence FromDateUtc(DateTime dateUtc)
    {
        if (dateUtc.Kind != DateTimeKind.Utc)
            throw new ValidationException("DateUtc", "DateUtc must be UTC.");

        return Create(dateUtc.Year, dateUtc.Month);
    }
}
