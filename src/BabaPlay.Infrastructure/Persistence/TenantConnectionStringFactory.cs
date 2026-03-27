using Microsoft.Data.SqlClient;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Builds a tenant SQL connection string from the platform connection string and tenant database name.
/// </summary>
public static class TenantConnectionStringFactory
{
    public static string ForDatabase(string platformConnectionString, string databaseName)
    {
        var sb = new SqlConnectionStringBuilder(platformConnectionString)
        {
            InitialCatalog = databaseName
        };
        return sb.ConnectionString;
    }
}
