namespace BabaPlay.Infrastructure.Settings;

public static class TenantLogoStorageProviders
{
    public const string Local = "Local";
    public const string Cloudinary = "Cloudinary";
}

public sealed class TenantLogoStorageSettings
{
    public const string SectionName = "TenantLogoStorage";

    public string Provider { get; init; } = TenantLogoStorageProviders.Local;

    public string LocalRootPath { get; init; } = "storage";

    public CloudinaryTenantLogoStorageSettings Cloudinary { get; init; } = new();
}

public sealed class CloudinaryTenantLogoStorageSettings
{
    public string CloudName { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string ApiSecret { get; init; } = string.Empty;

    public string Folder { get; init; } = "tenant-logos";
}
