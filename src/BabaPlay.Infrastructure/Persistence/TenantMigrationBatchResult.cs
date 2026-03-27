namespace BabaPlay.Infrastructure.Persistence;

public sealed record TenantMigrationBatchResult(
    int Total,
    int Migrated,
    int Failed,
    int SkippedEmptyDatabaseName);
