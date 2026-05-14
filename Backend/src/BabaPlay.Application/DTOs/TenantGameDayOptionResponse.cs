namespace BabaPlay.Application.DTOs;

public sealed record TenantGameDayOptionResponse(
    Guid Id,
    Guid TenantId,
    DayOfWeek DayOfWeek,
    TimeOnly LocalStartTime,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
