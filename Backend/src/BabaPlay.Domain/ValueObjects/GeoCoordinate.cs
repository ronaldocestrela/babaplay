using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Domain.ValueObjects;

public readonly record struct GeoCoordinate(double Latitude, double Longitude)
{
    public static GeoCoordinate Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ValidationException("Latitude", "Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ValidationException("Longitude", "Longitude must be between -180 and 180.");

        return new GeoCoordinate(latitude, longitude);
    }

    public double DistanceToMeters(GeoCoordinate other)
    {
        const double earthRadiusMeters = 6371000;

        var lat1 = ToRadians(Latitude);
        var lat2 = ToRadians(other.Latitude);
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
            * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusMeters * c;
    }

    private static double ToRadians(double angle) => angle * Math.PI / 180d;
}
