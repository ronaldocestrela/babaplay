namespace BabaPlay.Application.DTOs;

public sealed record TenantGeolocationSettingsDto(
    double Latitude,
    double Longitude,
    double CheckinRadiusMeters);
